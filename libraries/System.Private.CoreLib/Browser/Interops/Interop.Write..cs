// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Internal;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

internal static partial class Interop
{
    internal static partial class Sys
    {
        internal static unsafe partial int Write(SafeHandle fd, byte* buffer, int bufferSize)
        {
            //var handle = fd.DangerousGetHandle();
            //if (handle == 1) //Console Out handle
            //{
            //    var uint8Array = new Uint8Array(bytes);
            //    // 3. Decode as UTF-8 string
            //    const decodedString = new TextDecoder().decode(uint8Array);
            //    console.log(decodedString); // Output: Hello
            //}
            return -1;
        }

        internal static unsafe partial int Write(IntPtr fd, byte* buffer, int bufferSize)
        {
            return -1;
        }

        internal static unsafe partial int WriteToNonblocking(SafeHandle fd, byte* buffer, int bufferSize)
        {
            return -1;
        }
    }
}
