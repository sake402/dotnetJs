using dotnetJs.Translator;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Project = Microsoft.Build.Evaluation.Project;

namespace dotnetJs.Compiler
{
    public class ProjectWrapper : IProject
    {
        Project project;
        public CSharpCompilation? Compilation { get; }
        public string DirectoryPath => project.DirectoryPath;
        public string FullPath => project.FullPath;

        public ProjectWrapper(Project project)
        {
            this.project = project;
        }

        public string? Evaluate(string propertyName)
        {
            var v = project.GetPropertyValue(propertyName);
            if (!string.IsNullOrEmpty(v))
                return v;
            var value = project.AllEvaluatedProperties.LastOrDefault(e => e.Name == propertyName);
            return value?.EvaluatedValue;
        }
        public string GetAssemblyName()
        {
            return project.AllEvaluatedProperties.Last(e => e.Name == "AssemblyName").EvaluatedValue;
        }
        public string GetNamespace()
        {
            return project.AllEvaluatedProperties.Last(e => e.Name == "RootNamespace").EvaluatedValue;
        }
        public string GetOutputPath()
        {
            return project.AllEvaluatedProperties.Last(e => e.Name == "OutputPath").EvaluatedValue;
        }
        public OutputMode GetOutputMode()
        {
            var v = project.AllEvaluatedProperties.LastOrDefault(e => e.Name == "OutputMode")?.EvaluatedValue;
            Enum.TryParse<OutputMode>(v, out var value);
            if (value == OutputMode.None)
            {
                value = OutputMode.Global | OutputMode.InlineConstants | OutputMode.SingleFile;
            }
            if (value.HasFlag(OutputMode.Module) && value.HasFlag(OutputMode.Global))
            {
                throw new InvalidOperationException("Cannot enable both global and module at the same time");
            }
            if (!value.HasFlag(OutputMode.Module) && !value.HasFlag(OutputMode.Global))
            {
                value |= OutputMode.Global;
            }
            return value;
        }

        public IList<string> GetSourceFiles()
        {
            IList<string> sourceFiles = new List<string>();

            foreach (var projectItem in project.AllEvaluatedItems.Where(i => i.ItemType == "Compile"))
            {
                if (projectItem.EvaluatedInclude.Contains(".NETCoreApp,"))
                    continue;
                if (projectItem.EvaluatedInclude.Contains(':')) //check if it has volume label already
                    sourceFiles.Add(projectItem.EvaluatedInclude);
                else
                    sourceFiles.Add(Path.Join(project.DirectoryPath, projectItem.EvaluatedInclude));
            }

            return sourceFiles;
        }

        public IList<string> GetContentFiles()
        {
            IList<string> sourceFiles = new List<string>();

            foreach (var projectItem in project.AllEvaluatedItems.Where(i => i.ItemType == "Content"))
            {
                if (projectItem.EvaluatedInclude.Contains(".NETCoreApp,"))
                    continue;
                if (projectItem.EvaluatedInclude.Contains(':')) //check if it has volume label already
                    sourceFiles.Add(projectItem.EvaluatedInclude);
                else
                    sourceFiles.Add(Path.Join(project.DirectoryPath, projectItem.EvaluatedInclude));
            }

            return sourceFiles;
        }

        public IList<string> GetLinkerFiles()
        {
            IList<string> sourceFiles = new List<string>();

            foreach (var projectItem in project.AllEvaluatedItems.Where(i => i.ItemType == "ILLinkSubstitutionsXmls"))
            {
                if (projectItem.EvaluatedInclude.Contains(".NETCoreApp,"))
                    continue;
                if (projectItem.EvaluatedInclude.Contains(':')) //check if it has volume label already
                    sourceFiles.Add(projectItem.EvaluatedInclude);
                else
                    sourceFiles.Add(Path.Join(project.DirectoryPath, projectItem.EvaluatedInclude));
            }

            return sourceFiles;
        }

        public bool Build()
        {
            return project.Build();
        }
    }
}
