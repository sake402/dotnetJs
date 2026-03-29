namespace NetJs.Translator
{
    [Flags]
    public enum OutputMode
    {
        None,
        Module = 1 << 0,
        Global = 1 << 1,
        SingleFile = 1 << 2,
        SingleHtmlFile = 1 << 3,
        NoReflection = 1 << 4,
        SeparateReflectionModule = 1 << 5,
        ShortNames = 1 << 6,
        ShortNamesTryUseCamelCase = 1 << 7,
        InlineConstants = 1 << 8
    }
}
