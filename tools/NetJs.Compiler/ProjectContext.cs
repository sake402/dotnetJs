using System;
using System.IO;

class ProjectContext
{
    public ProjectContext(FileSystemWatcher razorWatcher, FileSystemWatcher csWatcher)
    {
        RazorWatcher = razorWatcher;
        CsWatcher = csWatcher;
    }

    public DateTime LastProcessed { get; set; }
    public FileSystemWatcher RazorWatcher { get; }
    public FileSystemWatcher CsWatcher { get; }
}
//var code = result.ToString();
//Console.WriteLine(code);
