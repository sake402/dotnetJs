namespace dotnetJs.Translator.CSharpToJavascript
{
    public struct AssemblySymbolDescriptor
    {
        public AssemblySymbolDescriptor()
        {
        }

        public Dictionary<string, string> Types { get; set; } = new();
        public Dictionary<string, Dictionary<string, string>> Members { get; set; } = new();
        public List<ILLinkerAssembly> LinkerSubstitutions { get; set; } = new();
    }
}