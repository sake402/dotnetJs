using System.Runtime.InteropServices;

namespace System.Reflection
{
    [NetJs.Boot]
    //[NetJs.Reflectable(false)]
    public abstract partial class MethodInfo
    {
        internal MethodInfo(MethodModel model) : base(model)
        {
        }
    }
}
