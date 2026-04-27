namespace System
{
    [NetJs.ForcePartial(typeof(Int16))]
    [NetJs.StaticCallConvention]
    public readonly struct Int16_Partial 
    {
        [NetJs.MemberReplace(nameof(GetHashCode))]
        public int GetHashCodeImplChar()
        {
            return this.As<int>();
        }

        readonly short _m_value;
        [NetJs.MemberReplace("m_value")]
        internal short MValue
        {
            get
            {
                if (NetJs.Script.TypeOf(this).NativeEquals("number"))
                    return this.As<short>();
                return _m_value;
            }
            set
            {
                NetJs.Script.Write("this._m_value = value");
            }
        }
    }
}
