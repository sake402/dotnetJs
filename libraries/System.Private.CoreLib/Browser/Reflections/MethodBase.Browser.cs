using System;
using System.Collections.Generic;
using System.Text;

namespace System.Reflection
{
    [NetJs.Boot]
    [NetJs.Reflectable(false)]
    public partial class MethodBase
    {
        [NetJs.MemberReplace(nameof(GetCurrentMethod))]
        public static MethodBase? GetCurrentMethodImpl()
        {
            return null;
        }
    }
}
