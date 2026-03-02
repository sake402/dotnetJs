using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

internal static partial class Interop
{
    internal static partial class Sys
    {
        private static partial int PathConf(string path, PathConfName name)
        {
            return -1;
        }

        internal static partial int FStat(SafeHandle fd, out FileStatus output)
        {
            output = default;
            return -1;
        }

        internal static partial int Stat(string path, out FileStatus output)
        {
            output = default;
            return -1;
        }

        internal static partial int LStat(string path, out FileStatus output)
        {
            output = default;
            return -1;
        }

    }

    internal static partial class ZLib
    {
        internal static unsafe partial ZLibNative.ErrorCode DeflateInit2_(
            ZLibNative.ZStream* stream,
            ZLibNative.CompressionLevel level,
            ZLibNative.CompressionMethod method,
            int windowBits,
            int memLevel,
            ZLibNative.CompressionStrategy strategy)
        {
            throw new PlatformNotSupportedException();
        }

        internal static unsafe partial ZLibNative.ErrorCode Deflate(ZLibNative.ZStream* stream, ZLibNative.FlushCode flush)
        {
            throw new PlatformNotSupportedException();
        }

        internal static unsafe partial ZLibNative.ErrorCode DeflateEnd(ZLibNative.ZStream* stream)
        {
            throw new PlatformNotSupportedException();
        }

        internal static unsafe partial ZLibNative.ErrorCode InflateInit2_(ZLibNative.ZStream* stream, int windowBits)
        {
            throw new PlatformNotSupportedException();
        }

        internal static unsafe partial ZLibNative.ErrorCode InflateReset2_(ZLibNative.ZStream* stream, int windowBits)
        {
            throw new PlatformNotSupportedException();
        }

        internal static unsafe partial ZLibNative.ErrorCode Inflate(ZLibNative.ZStream* stream, ZLibNative.FlushCode flush)
        {
            throw new PlatformNotSupportedException();
        }

        internal static unsafe partial ZLibNative.ErrorCode InflateEnd(ZLibNative.ZStream* stream)
        {
            throw new PlatformNotSupportedException();
        }

        internal static unsafe partial uint crc32(uint crc, byte* buffer, int len)
        {
            throw new PlatformNotSupportedException();
        }
    }

}
