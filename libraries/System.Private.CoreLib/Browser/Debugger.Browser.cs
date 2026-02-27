using System;
using System.Collections.Generic;
using System.Text;

namespace System.Diagnostics
{
    public static partial class Debugger
    {
        [dotnetJs.MemberReplace(nameof(IsAttached_internal))]
        private static bool IsAttached_internalImpl()
        {
            return false;
        }

        [dotnetJs.MemberReplace(nameof(Break))]
        public static void BreakImpl()
        {
            dotnetJs.Script.Write("debugger");
        }
        
        [dotnetJs.MemberReplace(nameof(IsLogging))]
        public static bool IsLoggingImpl()
        {
            return true;
        }
    }
}
