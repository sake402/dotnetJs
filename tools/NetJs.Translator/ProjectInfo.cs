//using System.IO;
//using System.Linq;
//using System.Text.RegularExpressions;

//namespace dotnetJs.Translator
//{
//    public class ProjectInfo
//    {
//        public string Path { get; set; } = default!;
//        public string FileName { get; set; } = default!;
//        public string Name => System.IO.Path.GetFileNameWithoutExtension(FileName);
//        public string Folder => System.IO.Path.GetDirectoryName(FileName);
//        public string Namespace { get; set; } = default!;
//        public string? SDK { get; set; }
//        public string Type { get; set; } = default!;
//        public string AssemblyName { get; set; } = default!;

//        internal static ProjectInfo? GetProjectDefinition(string path, int depth = 0)
//        {
//            if (depth > 10)
//                return null;
//            string? fileName = path.EndsWith(".csproj") ? path : null;
//            //Console.WriteLine($"Looking for project file in {path}");
//            if (fileName != null || Directory.EnumerateFiles(path).Any(f =>
//            {
//                if (f.EndsWith(".csproj"))
//                {
//                    fileName = f;
//                    return true;
//                }
//                return false;
//            }) && fileName != null)
//            {
//                //Console.WriteLine($"Project file found in {path}");
//                var csproj = File.ReadAllText(fileName);
//                var match = Regex.Match(csproj, ".?Sdk=\"(.*)\".?");
//                string? sdk = null;
//                if (match.Success)
//                {
//                    sdk = match.Groups[1].Value;
//                }
//                match = Regex.Match(csproj, ".?<TargetFramework>(.+)</TargetFramework>.?");
//                string projectType = "netstandard2.1";
//                if (match.Success)
//                {
//                    projectType = match.Groups[1].Value;
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
//                else
//                {
//                    assembly = @namespace;
//                }
//                return new ProjectInfo()
//                {
//                    Path = System.IO.Path.GetFullPath(path).Trim('/', '\\'),
//                    FileName = fileName,
//                    Namespace = @namespace,
//                    Type = projectType,
//                    SDK = sdk,
//                    AssemblyName = assembly
//                };
//            }
//            path = path.Trim(new char[] { '/', '\\' }) + "/../";
//            return GetProjectDefinition(path, depth + 1);
//        }
//    }
//}
