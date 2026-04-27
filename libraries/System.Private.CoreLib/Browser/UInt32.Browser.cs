
namespace System
{
    [NetJs.ForcePartial(typeof(UInt32))]
    [NetJs.StaticCallConvention]
    public readonly partial struct UInt32_Partial
    {
        [NetJs.MemberReplace(nameof(GetHashCode))]
        [NetJs.Template("{global.}" + NetJs.Constants.GetHashCodeName + "({this:!super})")] //make sure we dont pass super keyword in here. JS doesnt support it
        public int GetHashCodeImplChar()
        {
            return this.As<int>();
        }

        readonly uint _m_value;
        [NetJs.MemberReplace("m_value")]
        internal uint MValue
        {
            get
            {
                if (NetJs.Script.TypeOf(this).NativeEquals("number"))
                    return this.As<uint>();
                return _m_value;
            }
            set
            {
                NetJs.Script.Write("this._m_value = value");
            }
        }
    }
}
