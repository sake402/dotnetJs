namespace NetJs.Translator.RazorToCSharp
{
    [Flags]
    public enum HtmlParseState
    {
        StartOpenend, //<
        TagName,
        TagNamed,
        AttributeKey,
        AttributeKeyed,
        AttributeValue,
        AttributeValued,
        StartClosed, //>
        EndOpened, //</
        EndClosed, //>

        Code = 0x1000000,
        Quoted = 0x2000000,
        Flags = Code | Quoted
    }
}
