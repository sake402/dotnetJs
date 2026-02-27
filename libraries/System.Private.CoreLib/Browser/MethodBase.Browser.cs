using System;
using System.Collections.Generic;
using System.Text;

namespace System.Reflection
{
    public partial class MethodBase
    {
        [dotnetJs.MemberReplace(nameof(GetCurrentMethod))]
        public static MethodBase? GetCurrentMethodImpl()
        {
            return null;
        }
    }
}
