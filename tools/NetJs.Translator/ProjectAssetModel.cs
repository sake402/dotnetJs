using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetJs.Translator
{

    public partial class ProjectAssetModel
    {
        [JsonPropertyName("version")]
        public long Version { get; set; }

        [JsonPropertyName("targets")]
        public Dictionary<string, Dictionary<string, GraphLibrary>> Targets { get; set; }

        [JsonPropertyName("libraries")]
        public Dictionary<string, Library> Libraries { get; set; }

        [JsonPropertyName("projectFileDependencyGroups")]
        public Dictionary<string, string[]> ProjectFileDependencyGroups { get; set; }

        [JsonPropertyName("packageFolders")]
        public Dictionary<string, object> PackageFolders { get; set; }

        //[JsonPropertyName("project")]
        //public Project Project { get; set; }
    }

    public partial class GraphLibrary
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("framework")]
        public string Framework { get; set; }

        [JsonPropertyName("dependencies")]
        public Dictionary<string, string> Dependencies { get; set; }

        //[JsonPropertyName("compile")]
        //public Compile Compile { get; set; }

        //[JsonPropertyName("runtime")]
        //public Compile Runtime { get; set; }
    }

    public partial class Library
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("msbuildProject")]
        public string MsbuildProject { get; set; }
    }

}
