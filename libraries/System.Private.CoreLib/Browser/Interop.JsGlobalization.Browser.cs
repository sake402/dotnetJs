using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class JsGlobalization
    {
        [dotnetJs.MemberReplace(nameof(GetLocaleInfo))]
        internal static unsafe nint GetLocaleInfoImpl(char* locale, int localeLength, char* culture, int cultureLength, char* buffer, int bufferLength, out int resultLength)
        {
            resultLength = 0;
            return 1;
        }

    }
}
