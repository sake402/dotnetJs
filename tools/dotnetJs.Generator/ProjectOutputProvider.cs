using dotnetJs.Translator;
using dotnetJs.Translator.CSharpToJavascript;
using System.Collections.Generic;
using System.IO;

namespace dotnetJs.Generator
{
    public class ProjectOutputProvider : IProjectOutputProvider
    {
        const string GeneratedFolderName = "__dotnetJs";
        IProject project;
        public string OutputPath => Path.Combine(project.DirectoryPath, project.GetOutputPath(), GeneratedFolderName);
        public Stream HtmlScriptContent => htmlScriptContent;
        public Stream HtmlStyleContent => htmlStyleContent;
        public Stream HtmlBodyContent => htmlBodyContent;
        public IEnumerable<string> OutputtedFiles => outputtedFiles;

        List<string> outputtedFiles = new();
        MemoryStream htmlScriptContent = new MemoryStream();
        MemoryStream htmlStyleContent = new MemoryStream();
        MemoryStream htmlBodyContent = new MemoryStream();

        public ProjectOutputProvider(IProject project)
        {
            this.project = project;
        }

        public void Output(GlobalCompilationVisitor global, string destinationRelativePath, Stream content)
        {
            if (!global.OutputMode.HasFlag(OutputMode.SingleHtmlFile) || destinationRelativePath.EndsWith(".html"))
            {
                var outputFile = Path.Combine(OutputPath, "js", destinationRelativePath);
                var dir = Path.GetDirectoryName(outputFile);
                if (dir != null && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                var output = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
                content.CopyTo(output);
                output.Flush();
                output.Close();
                //File.WriteAllText(outputFile, content);
            }
            else
            {
                if (destinationRelativePath.EndsWith(".js"))
                    content.CopyTo(htmlScriptContent);
                else if (destinationRelativePath.EndsWith(".css"))
                    content.CopyTo(htmlStyleContent);
                else
                    content.CopyTo(htmlBodyContent);
            }
            outputtedFiles.Add(destinationRelativePath);
        }
    }
}
