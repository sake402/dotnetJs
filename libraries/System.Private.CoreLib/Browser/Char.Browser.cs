using NetJs;

namespace System
{
    [NetJs.ForcePartial(typeof(Char))]
    [NetJs.StaticCallConvention]
    public readonly struct Char_Partial //: ForcedPartialBase<Char>
    {
        [NetJs.MemberReplace("m_value")]
        [NetJs.Template("{this}")]
        private readonly char m_value;

        [NetJs.MemberReplace(nameof(GetHashCode))]
        [NetJs.Template("{global.}" + NetJs.Constants.GetHashCodeName + "({this:!super})")] //make sure we dont pass super keyword in here. JS doesnt support it
        public int GetHashCodeImplChar()
        {
            return this.As<char>();
        }
    }
}
