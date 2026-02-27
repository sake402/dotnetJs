//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.

//using System;
//using System.IO.Compression;
//using System.Runtime.InteropServices;
//using Microsoft.Win32.SafeHandles;

//#if BROWSER

//internal static partial class Interop
//{
//    internal static partial class Brotli
//    {
//        internal static partial SafeBrotliDecoderHandle BrotliDecoderCreateInstance(IntPtr allocFunc, IntPtr freeFunc, IntPtr opaque)
//        {
//            throw new PlatformNotSupportedException();
//        }

//        internal static unsafe partial int BrotliDecoderDecompressStream(
//            SafeBrotliDecoderHandle state, ref nuint availableIn, byte** nextIn,
//            ref nuint availableOut, byte** nextOut, out nuint totalOut)
//        {
//            throw new PlatformNotSupportedException();
//        }

//        internal static unsafe partial BOOL BrotliDecoderDecompress(nuint availableInput, byte* inBytes, nuint* availableOutput, byte* outBytes)
//        {
//            throw new PlatformNotSupportedException();
//        }

//        internal static partial void BrotliDecoderDestroyInstance(IntPtr state)
//        {
//            throw new PlatformNotSupportedException();
//        }

//        internal static partial BOOL BrotliDecoderIsFinished(SafeBrotliDecoderHandle state)
//        {
//            throw new PlatformNotSupportedException();
//        }

//        internal static partial SafeBrotliEncoderHandle BrotliEncoderCreateInstance(IntPtr allocFunc, IntPtr freeFunc, IntPtr opaque)
//        {
//            throw new PlatformNotSupportedException();
//        }

//        internal static partial BOOL BrotliEncoderSetParameter(SafeBrotliEncoderHandle state, BrotliEncoderParameter parameter, uint value)
//        {
//            throw new PlatformNotSupportedException();
//        }

//        internal static unsafe partial BOOL BrotliEncoderCompressStream(
//            SafeBrotliEncoderHandle state, BrotliEncoderOperation op, ref nuint availableIn,
//            byte** nextIn, ref nuint availableOut, byte** nextOut, out nuint totalOut)
//        {
//            throw new PlatformNotSupportedException();
//        }

//        internal static partial BOOL BrotliEncoderHasMoreOutput(SafeBrotliEncoderHandle state)
//        {
//            throw new PlatformNotSupportedException();
//        }

//        internal static partial nuint BrotliEncoderMaxCompressedSize(nuint inputSize)
//        {
//            throw new PlatformNotSupportedException();
//        }

//        internal static partial void BrotliEncoderDestroyInstance(IntPtr state)
//        {
//            throw new PlatformNotSupportedException();
//        }

//        internal static unsafe partial BOOL BrotliEncoderCompress(int quality, int window, int v, nuint availableInput, byte* inBytes, nuint* availableOutput, byte* outBytes)
//        {
//            throw new PlatformNotSupportedException();
//        }
//    }
//}

//#endif