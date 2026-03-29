using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static partial class Environment
    {
        [NetJs.MemberReplace(nameof(ExitCode))]
        public static int ExitCodeImpl { get; set; }

        [NetJs.MemberReplace(nameof(GetProcessorCount))]
        internal static int GetProcessorCountImpl()
        {
            return 1;
        }
        
        [NetJs.MemberReplace(nameof(Exit))]
        public static void ExitImpl(int exitCode)
        {

        }

        [NetJs.MemberReplace(nameof(GetCommandLineArgs))]
        public static string[] GetCommandLineArgsImpl()
        {
            return [];
        }

        [NetJs.MemberReplace(nameof(FailFast))]
        internal static void FailFastImpl(string? message, Exception? exception, string? errorSource)
        {

        }
        
        public static string Version => "1.0";
    }
}
