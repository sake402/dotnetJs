using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    internal static partial class SpanHelpers
    {
        [NetJs.MemberReplace(nameof(Memmove))]
        internal static void MemmoveImpl(ref byte dest, ref byte src, nuint len)
        {
            MemmoveNative(ref dest, ref src, len);
        }

        [NetJs.MemberReplace(nameof(memmove))]
        private static unsafe void memmoveImpl(void* dest, void* src, nuint len)
        {
            Unsafe.CopyBlockFinal(dest, src, len);
        }
    }
}
