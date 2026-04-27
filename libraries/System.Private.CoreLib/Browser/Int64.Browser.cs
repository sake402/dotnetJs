
namespace System
{
    [NetJs.ForcePartial(typeof(Int64))]
    [NetJs.StaticCallConvention]
    public readonly partial struct Int64_Partial
    {
        [NetJs.MemberReplace(nameof(GetHashCode))]
        [NetJs.Template("{global.}" + NetJs.Constants.GetHashCodeName + "({this:!super})")] //make sure we dont pass super keyword in here. JS doesnt support it
        public int GetHashCodeImplChar()
        {
            return (int)(this.As<long>());
        }

        readonly long _m_value;
        [NetJs.MemberReplace("m_value")]
        internal long MValue
        {
            get
            {
                if (NetJs.Script.TypeOf(this).NativeEquals("number"))
                    return this.As<long>();
                return _m_value;
            }
            set
            {
                NetJs.Script.Write("this._m_value = value");
            }
        }
    }
}
