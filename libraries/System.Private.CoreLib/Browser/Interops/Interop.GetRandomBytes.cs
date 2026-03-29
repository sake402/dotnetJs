using NetJs;
using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {       

        internal static unsafe partial void GetNonCryptographicallySecureRandomBytes(byte* buffer, int length)
        {
            for (int i = 0; i < length; i++)
            {
                buffer[i] = (byte)Math.Floor(Math.Random() * 256); // Generate a random integer between 0 and 255
            }
        }

        internal static unsafe partial int GetCryptographicallySecureRandomBytes(byte* buffer, int length)
        {
            var uint8Arr = Script.Write<object>("new Uint8Array(length)");
            Script.Write("window.crypto.getRandomValues(uint8Arr)");
            var array = Script.Write<byte[]>("Array.from(uint8Arr)");
            for (int i = 0; i < length; i++)
            {
                buffer[i] = array[i];
            }
            return 0;
        }

    }
}
