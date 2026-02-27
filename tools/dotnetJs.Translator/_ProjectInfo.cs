//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Security.Principal;
//using System.Text.RegularExpressions;

//namespace BlazorJs.Compiler
//{
//    public class ProjectInfo
//    {
//        public string Path { get; set; }
//        public string FileName { get; set; }
//        public string Namespace { get; set; }
//        public string? SDK { get; set; }
//        public string Type { get; set; }
//        public string AssemblyName { get; set; }
//        public Dictionary<string, List<string>>? Includes { get; set; }
//        internal static ProjectInfo? GetProjectDefinition(string path, string? exportIncludes = null, int depth = 0)
//        {
//            if (depth > 10)
//                return null;
//            string? fileName = null;
//            if (Directory.EnumerateFiles(path).Any(f =>
//            {
//                if (f.EndsWith(".csproj"))
//                {
//                    fileName = f;
//                    return true;
//                }
//                return false;
//            }))
//            {
//                //Console.WriteLine($"Project file found in {path}");
//                var csproj = File.ReadAllText(fileName);
//                var match = Regex.Match(csproj, ".?<TargetFramework>(.+)</TargetFramework>.?");
//                string projectType = "netstandard2.1";
//                if (match.Success)
//                {
//                    projectType = match.Groups[1].Value;
//                }
//                string? sdk = null;
//                if (match.Success)
//                {
//                    sdk = match.Groups[1].Value;
//                }
//                match = Regex.Match(csproj, ".?<RootNamespace>(.+)</RootNamespace>.?");
//                string @namespace = System.IO.Path.GetFileNameWithoutExtension(fileName);
//                if (match.Success)
//                {
//                    @namespace = match.Groups[1].Value;
//                }
//                match = Regex.Match(csproj, ".?<AssemblyName>(.+)</AssemblyName>.?");
//                string? assembly = null;
//                if (match.Success)
//                {
//                    assembly = match.Groups[1].Value;
//                }
//                Dictionary<string, List<string>>? includes = null;
//                if (exportIncludes != null)
//                {
//                    includes ??= new Dictionary<string, List<string>>();
//                    var exports = exportIncludes.Split(',');
//                    foreach (var e in exports)
//                    {
//                        List<string> values = new List<string>();
//                        match = Regex.Match(csproj, $".?<{e} Include=\"(.+)\" />.?");
//                        if (match.Success)
//                        {
//                            for (int ii = 1; ii < match.Groups.Count; ii++)
//                            {
//                                values.Add(match.Groups[ii].Value);
//                            }
//                        }
//                        includes[e] = values;
//                    }
//                }
//                return new ProjectInfo()
//                {
//                    Path = System.IO.Path.GetFullPath(path).Trim('/', '\\'),
//                    FileName = fileName,
//                    Namespace = @namespace,
//                    Type = projectType,
//                    SDK = sdk,
//                    AssemblyName = assembly,
//                    Includes = includes
//                };
//            }
//            path = path.Trim(new char[] { '/', '\\' }) + "/../";
//            return GetProjectDefinition(path, exportIncludes, depth + 1);
//        }
//    }
//}
