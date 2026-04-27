using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace NetJs.Tests
{
    internal class NumericTests
    {
        public static void Run()
        {
            TestIntegerArithmetic();
            TestUnsignedArithmetic();
            TestLongArithmetic();
            TestULongArithmetic();
            TestDecimalArithmetic();
            TestFloatingPointArithmetic();
            TestMixedConversions();
            TestBitwise();
            TestLongBitwise();
            TestULongBitwise();
            TestMathFunctions();
            TestComparisons();
            //TestOverflowChecked();
            //TestBigIntegerInterop();
            Console.WriteLine("✅ Numeric Tests passed.");
        }

        private static void TestIntegerArithmetic()
        {
            int a = 42, b = 10;
            Debug.Assert(a + b == 52);
            Debug.Assert(a - b == 32);
            Debug.Assert(a * b == 420);
            Debug.Assert(a / b == 4);
            Debug.Assert(a % b == 2);
            Debug.Assert((-a) == -42);
        }

        private static void TestUnsignedArithmetic()
        {
            uint a = 3000000000u, b = 1000000000u;
            Debug.Assert(a + b == 4000000000u); // overflow wraps around
            Debug.Assert(a - b == 2000000000u);
            Debug.Assert(a / b == 3u);
            Debug.Assert(a % b == 0u);
        }

        private static void TestLongArithmetic()
        {
            long a = 9_000_000_000L;
            long b = 2_000_000_000L;
            Debug.Assert(a + b == 11_000_000_000L);
            Debug.Assert(a - b == 7_000_000_000L);
            Debug.Assert(a / b == 4L);
            Debug.Assert(a % b == 1_000_000_000L);
            Debug.Assert(-a == -9_000_000_000L);

            //unchecked
            //{
            //    long x = long.MaxValue + 1;
            //    Debug.Assert(x == long.MinValue);
            //}
        }

        private static void TestULongArithmetic()
        {
            ulong a = 18_000_000_000_000_000_000UL;
            ulong b = 2_000_000_000_000_000_000UL;
            //Debug.Assert(a + b == unchecked(20_000_000_000_000_000_000UL));
            Debug.Assert(a - b == 16_000_000_000_000_000_000UL);
            Debug.Assert(a / b == 9UL);
            Debug.Assert(a % b == 0UL);

            //unchecked
            //{
            //    ulong wrap = ulong.MaxValue + 1;
            //    Debug.Assert(wrap == 0UL);
            //}
        }

        private static void TestDecimalArithmetic()
        {
            //decimal x = 10.5m, y = 2.25m;
            //Debug.Assert(x + y == 12.75m);
            //Debug.Assert(x - y == 8.25m);
            //Debug.Assert(x * y == 23.625m);
            //Debug.Assert(x / y == 4.6666666666666666666666666667m);
            //Debug.Assert(decimal.Remainder(10m, 3m) == 1m);

            //decimal neg = -123.45m;
            //Debug.Assert(decimal.Abs(neg) == 123.45m);
            //Debug.Assert(decimal.Round(12.3456m, 2) == 12.35m);
            //Debug.Assert(decimal.Truncate(12.999m) == 12m);
        }
        
        private static void TestFloatingPointArithmetic()
        {
            double x = 5.5, y = 2.0;
            Debug.Assert(Math.Abs((x + y) - 7.5) < 1e-12);
            Debug.Assert(Math.Abs((x - y) - 3.5) < 1e-12);
            Debug.Assert(Math.Abs((x * y) - 11.0) < 1e-12);
            Debug.Assert(Math.Abs((x / y) - 2.75) < 1e-12);

            float f = 1.5f, g = 2.5f;
            Debug.Assert(Math.Abs((f + g) - 4.0f) < 1e-6);
            Debug.Assert(Math.Abs((f * g) - 3.75f) < 1e-6);

            Debug.Assert(double.IsNaN(Math.Sqrt(-1)));
            Debug.Assert(double.IsPositiveInfinity(1.0 / 0.0));
        }

        private static void TestMixedConversions()
        {
            int i = 123;
            double d = i;
            Debug.Assert(Math.Abs(d - 123.0) < 1e-12);

            double f = 12.7;
            int j = (int)f;
            Debug.Assert(j == 12);

            //long l = int.MaxValue + 1L;
            //Debug.Assert(l == 2147483648L);

            //decimal m = (decimal)d;
            //Debug.Assert(m == 123m);

            //double d2 = (double)m;
            //Debug.Assert(Math.Abs(d2 - 123.0) < 1e-12);
        }

        private static void TestBitwise()
        {
            int x = 0b1010, y = 0b1100;
            Debug.Assert((x & y) == 0b1000);
            Debug.Assert((x | y) == 0b1110);
            Debug.Assert((x ^ y) == 0b0110);
            Debug.Assert((~x & 0b1111) == 0b0101);
            Debug.Assert((x << 1) == 0b10100);
            Debug.Assert((y >> 2) == 0b0011);
            
            //ulong u = 1;
            //Debug.Assert((u << 63) == 9223372036854775808UL);
            //Debug.Assert((u << 64) == 1UL); // JS may differ – test for transpiler consistency
        }

        private static void TestLongBitwise()
        {
            long a = 0x0F0F0F0FL;
            long b = 0x00FF00FFL;

            Debug.Assert((a & b) == 0x000F000FL);
            Debug.Assert((a | b) == 0x0FFF0FFFL);
            Debug.Assert((a ^ b) == 0x0FF00FF0L);
            Debug.Assert(~a == -0x0F0F0F10L);

            Debug.Assert((a << 4) == 0xF0F0F0F0L);
            Debug.Assert((b >> 8) == 0x0000FF00L);

            long c = -1L;
            Debug.Assert((c >> 63) == -1L); // arithmetic shift right
            Debug.Assert((1L << 63) == long.MinValue);
            Debug.Assert(((1L << 63) >> 63) == -1L);
        }

        private static void TestULongBitwise()
        {
            ulong a = 0x0F0F0F0FUL;
            ulong b = 0x00FF00FFUL;

            Debug.Assert((a & b) == 0x000F000FUL);
            Debug.Assert((a | b) == 0x0FFF0FFFUL);
            Debug.Assert((a ^ b) == 0x0FF00FF0UL);
            Debug.Assert(~a == 0xFFFFFFFFF0F0F0F0UL);

            Debug.Assert((a << 8) == 0x0F0F0F0F00UL);
            Debug.Assert((b >> 4) == 0x000FF00FUL);

            Debug.Assert((1UL << 63) == 9223372036854775808UL);
            Debug.Assert(((1UL << 63) >> 63) == 1UL);

            ulong mask = 0xFFFFFFFFFFFFFFFFUL;
            Debug.Assert((mask & a) == a);
            Debug.Assert((mask ^ a) == ~a);
        }

        private static void TestMathFunctions()
        {
            double v = 9.0;
            Debug.Assert(Math.Sqrt(v) == 3.0);
            Debug.Assert(Math.Pow(2, 3) == 8.0);
            Debug.Assert(Math.Round(1.5) == 2.0);
            Debug.Assert(Math.Round(2.5) == 2.0);
            Debug.Assert(Math.Floor(2.9) == 2.0);
            Debug.Assert(Math.Ceiling(2.1) == 3.0);
            Debug.Assert(Math.Abs(-5) == 5);
        }

        private static void TestComparisons()
        {
            int a = 5, b = 8;
            Debug.Assert(a < b);
            Debug.Assert(b > a);
            Debug.Assert(a <= 5);
            Debug.Assert(b >= 8);
            Debug.Assert(a != b);
            Debug.Assert(5 == a);

            //long l1 = 100L, l2 = 200L;
            //Debug.Assert(l2 > l1);

            //decimal m1 = 10.5m, m2 = 10.5m, m3 = 9.9m;
            //Debug.Assert(m1 == m2);
            //Debug.Assert(m1 > m3);
        }

        //private static void TestOverflowChecked()
        //{
        //    checked
        //    {
        //        bool threw = false;
        //        try
        //        {
        //            int z = int.MaxValue + 1;
        //        }
        //        catch (OverflowException)
        //        {
        //            threw = true;
        //        }
        //        Debug.Assert(threw);
        //    }

        //    unchecked
        //    {
        //        int max = int.MaxValue;
        //        int wrap = max + 1;
        //        Debug.Assert(wrap == int.MinValue);
        //    }
        //}

        //private static void TestBigIntegerInterop()
        //{
        //    BigInteger a = new BigInteger(1234567890123456789);
        //    BigInteger b = new BigInteger(9876543210);
        //    var sum = a + b;
        //    Debug.Assert(sum > a);
        //    Debug.Assert(sum - b == a);
        //    Debug.Assert(BigInteger.Multiply(a, 2) == a * 2);
        //}
    }
}

