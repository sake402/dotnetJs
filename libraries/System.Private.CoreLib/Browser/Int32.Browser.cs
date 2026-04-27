
namespace System
{
    [NetJs.ForcePartial(typeof(Int32))]
    [NetJs.StaticCallConvention]
    public readonly partial struct Int32_Partial
    {
        [NetJs.MemberReplace(nameof(GetHashCode))]
        [NetJs.Template("{global.}" + NetJs.Constants.GetHashCodeName + "({this:!super})")] //make sure we dont pass super keyword in here. JS doesnt support it
        public int GetHashCodeImplChar()
        {
            return this.As<int>();
        }

        readonly int _m_value;
        [NetJs.MemberReplace("m_value")]
        internal int MValue
        {
            get
            {
                if (NetJs.Script.TypeOf(this).NativeEquals("number"))
                    return this.As<int>();
                return _m_value;
            }
            set
            {
                NetJs.Script.Write("this._m_value = value");
            }
        }
    }
}
