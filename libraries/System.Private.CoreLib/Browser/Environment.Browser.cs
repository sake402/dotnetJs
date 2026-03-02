using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static partial class Environment
    {
        [dotnetJs.MemberReplace(nameof(ExitCode))]
        public static int ExitCodeImpl { get; set; }

        [dotnetJs.MemberReplace(nameof(GetProcessorCount))]
        internal static int GetProcessorCountImpl()
        {
            return 1;
        }
        
        [dotnetJs.MemberReplace(nameof(Exit))]
        public static void ExitImpl(int exitCode)
        {

        }

        [dotnetJs.MemberReplace(nameof(GetCommandLineArgs))]
        public static string[] GetCommandLineArgsImpl()
        {
            return [];
        }

        [dotnetJs.MemberReplace(nameof(FailFast))]
        internal static void FailFastImpl(string? message, Exception? exception, string? errorSource)
        {

        }
        
        public static string Version => "1.0";
    }
}
