using dotnetJs.Translator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace dotnetJs.Generator
{
    public class Project : IProject
    {
        internal static Project? GetProjectDefinition(CSharpCompilation compilation, ImmutableArray<AdditionalText> additionalTexts, string? exportIncludes = null)
        {
            var firstSyntaxFile = compilation.SyntaxTrees.FirstOrDefault()?.FilePath;
            var path = Path.GetDirectoryName(firstSyntaxFile ?? "");
            return GetProjectDefinition(compilation, additionalTexts, path ?? "", exportIncludes, 0);
        }

        static Project? GetProjectDefinition(CSharpCompilation compilation, ImmutableArray<AdditionalText> additionalTexts, string folderPath, string? exportIncludes = null, int depth = 0)
        {
            if (depth > 10)
                return null;
            string? fileName = null;
            if (Directory.EnumerateFiles(folderPath).Any(f =>
            {
                if (f.EndsWith(".csproj"))
                {
                    fileName = f;
                    return true;
                }
                return false;
            }))
            {
                var csproj = File.ReadAllText(fileName);
                var match = Regex.Match(csproj, ".?<TargetFramework>(.+)</TargetFramework>.?");
                string projectType = "netstandard2.1";
                if (match.Success)
                {
                    projectType = match.Groups[1].Value;
                }
                match = Regex.Match(csproj, ".?<Configuration>(.+)</Configuration>.?");
                string configuration = System.IO.Path.GetFileNameWithoutExtension(fileName);
                if (match.Success)
                {
                    configuration = match.Groups[1].Value;
                }
                match = Regex.Match(csproj, ".?<ApplicationTitle>(.+)</ApplicationTitle>.?");
                string applicationTitle = System.IO.Path.GetFileNameWithoutExtension(fileName);
                if (match.Success)
                {
                    applicationTitle = match.Groups[1].Value;
                }
                match = Regex.Match(csproj, ".?<RootNamespace>(.+)</RootNamespace>.?");
                string @namespace = System.IO.Path.GetFileNameWithoutExtension(fileName);
                if (match.Success)
                {
                    @namespace = match.Groups[1].Value;
                }
                match = Regex.Match(csproj, ".?<AssemblyName>(.+)</AssemblyName>.?");
                string? assembly = null;
                if (match.Success)
                {
                    assembly = match.Groups[1].Value;
                }
                match = Regex.Match(csproj, ".?<OutputPath>(.+)</OutputPath>.?");
                string? outputPath = null;
                if (match.Success)
                {
                    outputPath = match.Groups[1].Value;
                }
                match = Regex.Match(csproj, ".?<OutputMode>(.+)</OutputMode>.?");
                OutputMode outputMode = OutputMode.Global | OutputMode.InlineConstants;
                if (match.Success)
                {
                    outputMode = (OutputMode)Enum.Parse(typeof(OutputMode), match.Groups[1].Value);
                }
                match = Regex.Match(csproj, ".?<Global>(.+)</Global>.?");
                string? globalName = null;
                if (match.Success)
                {
                    globalName = match.Groups[1].Value;
                }
                Dictionary<string, List<string>>? includes = null;
                if (exportIncludes != null)
                {
                    includes ??= new Dictionary<string, List<string>>();
                    var exports = exportIncludes.Split(',');
                    foreach (var e in exports)
                    {
                        List<string> values = new List<string>();
                        match = Regex.Match(csproj, $".?<{e} Include=\"(.+)\" />.?");
                        if (match.Success)
                        {
                            for (int ii = 1; ii < match.Groups.Count; ii++)
                            {
                                values.Add(match.Groups[ii].Value);
                            }
                        }
                        includes[e] = values;
                    }
                }
                return new Project(compilation, additionalTexts)
                {
                    DirectoryPath = Path.GetFullPath(folderPath).Trim('/', '\\'),
                    FullPath = fileName,
                    RootNamespace = @namespace,
                    Type = projectType,
                    AssemblyName = assembly,
                    OutputPath = outputPath,
                    OutputMode = outputMode,
                    Configuration = configuration,
                    ApplicationTitle = applicationTitle,
                    GlobalName = globalName,
                    Includes = includes
                };
            }
            folderPath = folderPath.Trim(new char[] { '/', '\\' }) + "/../";
            return GetProjectDefinition(compilation, additionalTexts, folderPath, exportIncludes, depth + 1);
        }


        CSharpCompilation compilation;
        ImmutableArray<AdditionalText> additionalTexts;
        public CSharpCompilation Compilation => compilation;
        public string DirectoryPath { get; set; }
        public string FullPath { get; set; }
        public string RootNamespace { get; set; }
        public string Type { get; set; }
        public string AssemblyName { get; set; }
        public string OutputPath { get; set; }
        public OutputMode OutputMode { get; set; }
        public string Configuration { get; set; }
        public string ApplicationTitle { get; set; }
        public string GlobalName { get; set; }
        public Dictionary<string, List<string>>? Includes { get; set; }
        public Project(CSharpCompilation compilation, ImmutableArray<AdditionalText> additionalTexts)
        {
            this.compilation = compilation;
            this.additionalTexts = additionalTexts;
            var firstSyntaxFile = compilation.SyntaxTrees.FirstOrDefault()?.FilePath;

            var dir = Path.GetDirectoryName(firstSyntaxFile ?? "");


            if (firstSyntaxFile != null)
            {
                DirectoryPath = Path.GetDirectoryName(firstSyntaxFile) ?? "";
                FullPath = firstSyntaxFile;
            }
            else
            {
                DirectoryPath = "";
                FullPath = "";
            }
        }

        public string? Evaluate(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Configuration):
                    return Configuration;
                case nameof(RootNamespace):
                    return RootNamespace;
                case nameof(ApplicationTitle):
                    return ApplicationTitle;
                case nameof(GlobalName):
                    return GlobalName;
            }
            return null;
        }
        public string GetAssemblyName() => AssemblyName;
        public string GetNamespace() => RootNamespace;
        public string GetOutputPath() => OutputPath;
        public OutputMode GetOutputMode() => OutputMode;

        public IList<string> GetSourceFiles()
        {
            IList<string> sourceFiles = new List<string>();
            foreach (var s in compilation.SyntaxTrees)
            {
                sourceFiles.Add(s.FilePath);
            }
            return sourceFiles;
        }

        public IList<string> GetContentFiles()
        {
            IList<string> sourceFiles = new List<string>();
            foreach (var s in additionalTexts)
            {
                sourceFiles.Add(s.Path);
            }
            return sourceFiles;
        }

        public IList<string> GetLinkerFiles()
        {
            IList<string> sourceFiles = new List<string>();
            foreach (var s in additionalTexts)
            {
                sourceFiles.Add(s.Path);
            }
            return sourceFiles;
        }

        public bool Build()
        {
            return false;
        }
    }
}
