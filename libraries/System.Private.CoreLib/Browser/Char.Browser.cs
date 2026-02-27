using dotnetJs;

namespace System
{
    [dotnetJs.ForcePartial(typeof(Char))]
    [dotnetJs.StaticCallConvention]
    public readonly struct Char_Partial //: ForcedPartialBase<Char>
    {
        [dotnetJs.MemberReplace("m_value")]
        [dotnetJs.Template("{this}")]
        private readonly char m_value;

        [dotnetJs.MemberReplace(nameof(GetHashCode))]
        [dotnetJs.Template("{global.}" + dotnetJs.Constants.GetHashCodeName + "({this:!super})")] //make sure we dont pass super keyword in here. JS doesnt support it
        public int GetHashCodeImplChar()
        {
            return this.As<char>();
        }
    }
}
