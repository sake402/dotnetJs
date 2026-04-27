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
        public Dictionary<string, Dictionary<string, GraphLibrary>> Targets { get; set; } = default!;

        [JsonPropertyName("libraries")]
        public Dictionary<string, Library> Libraries { get; set; } = default!;

        [JsonPropertyName("projectFileDependencyGroups")]
        public Dictionary<string, string[]> ProjectFileDependencyGroups { get; set; } = default!;

        [JsonPropertyName("packageFolders")]
        public Dictionary<string, object> PackageFolders { get; set; } = default!;

        //[JsonPropertyName("project")]
        //public Project Project { get; set; }
    }

    public partial class GraphLibrary
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = default!;

        [JsonPropertyName("framework")]
        public string Framework { get; set; } = default!;

        [JsonPropertyName("dependencies")]
        public Dictionary<string, string> Dependencies { get; set; } = default!;

        //[JsonPropertyName("compile")]
        //public Compile Compile { get; set; }

        //[JsonPropertyName("runtime")]
        //public Compile Runtime { get; set; }
    }

    public partial class Library
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = default!;

        [JsonPropertyName("path")]
        public string Path { get; set; } = default!;

        [JsonPropertyName("msbuildProject")]
        public string MsbuildProject { get; set; } = default!;
    }

}
