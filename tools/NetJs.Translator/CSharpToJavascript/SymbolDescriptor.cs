namespace NetJs.Translator.CSharpToJavascript
{
    public struct SymbolDescriptor
    {
        public SymbolDescriptor()
        {
        }
        public string? GlobalNamespace { get; set; }
        public Dictionary<string, string> Types { get; set; } = new();
        public Dictionary<string, Dictionary<string, string>> Members { get; set; } = new();
        public List<ILLinkerAssembly> LinkerSubstitutions { get; set; } = new();
    }
}