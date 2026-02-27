namespace dotnetJs.Translator.CSharpToJavascript
{
    public struct SymbolDescriptor
    {
        public SymbolDescriptor()
        {
        }

        public Dictionary<string, string> Types { get; set; } = new();
        public Dictionary<string, Dictionary<string, string>> Members { get; set; } = new();
        public List<ILLinkerAssembly> LinkerSubstitutions { get; set; } = new();
    }
}