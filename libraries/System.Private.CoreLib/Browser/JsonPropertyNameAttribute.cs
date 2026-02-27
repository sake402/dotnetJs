using System;
using System.Collections.Generic;
using System.Text;

namespace System.Text.Json.Serialization
{
    internal class JsonPropertyNameAttribute : Attribute
    {
        public JsonPropertyNameAttribute(string s) { }
    }

    internal class JsonIgnoreAttribute : Attribute
    {
    }
}
