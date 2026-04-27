using NetJs;

namespace System
{
    [NetJs.ForcePartial(typeof(Char))]
    [NetJs.StaticCallConvention]
    public readonly struct Char_Partial
    {
        [NetJs.MemberReplace(nameof(GetHashCode))]
        public int GetHashCodeImplChar()
        {
            return this.As<char>();
        }

        readonly char _m_value;
        [NetJs.MemberReplace("m_value")]
        internal char MValue
        {
            get
            {
                if (NetJs.Script.TypeOf(this).NativeEquals("number"))
                    return this.As<char>();
                return _m_value;
            }
            set
            {
                NetJs.Script.Write("this._m_value = value");
            }
        }
    }
}
