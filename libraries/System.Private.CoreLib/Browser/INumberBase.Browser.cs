namespace System.Numerics
{
    [NetJs.ForcePartial(typeof(INumberBase<>))]
    public partial interface INumberBase_Partial<TSelf>
    {
        [NetJs.Name(NetJs.Constants.IsTypeName)]
        public static bool Is(object value)
        {
            return NetJs.Script.TypeOf(value).NativeEquals("number");
        }
    }
}