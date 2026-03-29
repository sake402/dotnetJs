using NetJs;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;


namespace System.Threading
{
    public static partial class Interlocked
    {
        [NetJs.MemberReplace(nameof(CompareExchange) + "(ref int, int, int)")]
        public static int CompareExchangeImpl(ref int location1, int value, int comparand)
        {
            var v = location1;
            if (v == comparand)
            {
                location1 = value;
            }
            return v;
        }

        [NetJs.MemberReplace(nameof(CompareExchange) + "(ref object?, ref object?, ref object?, ref object?)")]
        private static void CompareExchangeImpl(ref object? location1, ref object? value, ref object? comparand, [NotNullIfNotNull(nameof(location1))] ref object? result)
        {
            var v = location1;
            if (v == comparand)
            {
                location1 = value;
            }
            result = v;
        }

        [NetJs.MemberReplace(nameof(Decrement) + "(ref int)")]
        public static int DecrementImpl(ref int location)
        {
            location--;
            return location;
        }

        [NetJs.MemberReplace(nameof(Decrement) + "(ref long)")]
        public static long DecrementImpl(ref long location)
        {
            location--;
            return location;
        }

        [NetJs.MemberReplace(nameof(Increment) + "(ref int)")]
        public static int IncrementImpl(ref int location)
        {
            location++;
            return location;
        }

        [NetJs.MemberReplace(nameof(Increment) + "(ref long)")]
        public static long IncrementImpl(ref long location)
        {
            location++;
            return location;
        }

        [NetJs.MemberReplace(nameof(Exchange) + "(ref int, int)")]
        public static int ExchangeImpl(ref int location1, int value)
        {
            var v = location1;
            location1 = value;
            return v;
        }

        [NetJs.MemberReplace(nameof(Exchange) + "(ref object?, ref object?, ref object?)")]
        private static void ExchangeImpl([NotNullIfNotNull(nameof(value))] ref object? location1, ref object? value, [NotNullIfNotNull(nameof(location1))] ref object? result)
        {
            var v = location1;
            location1 = value;
            result = v;
        }


        [NetJs.MemberReplace(nameof(CompareExchange) + "(ref long, long, long)")]
        public static long CompareExchangeImpl(ref long location1, long value, long comparand)
        {
            var v = location1;
            location1 = value;
            return v;
        }

        [NetJs.MemberReplace(nameof(Exchange) + "(ref long, long)")]
        public static long ExchangeImpl(ref long location1, long value)
        {
            var v = location1;
            location1 = value;
            return v;
        }

        [NetJs.MemberReplace(nameof(Read) + "(ref long)")]
        public static long ReadImpl(ref long location)
        {
            return location;
        }

        [NetJs.MemberReplace(nameof(Add) + "(ref int, int)")]
        public static int AddImpl(ref int location1, int value)
        {
            location1 += value;
            return location1;
        }

        [NetJs.MemberReplace(nameof(Add) + "(ref long, long)")]
        public static long AddImpl(ref long location1, long value)
        {
            location1 += value;
            return location1;
        }

        [NetJs.MemberReplace(nameof(MemoryBarrierProcessWide))]
        public static void MemoryBarrierProcessWideImpl()
        {

        }
    }
}
