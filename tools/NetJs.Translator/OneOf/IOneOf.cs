namespace NetJs.Translator.OneOf
{
    public interface IOneOf //: IValueProvider
    { 
        object Value { get ; }
        int Index { get; }
    }
}