//using System;
//using System.Collections.Generic;
//using System.Diagnostics.Contracts;

//namespace TranslationTest
//{
//    //public enum TestEnum
//    //{
//    //    WriteThrough = unchecked((int)0x80000000),
//    //    Asynchronous = unchecked((int)0x40000000),
//    //}
    
//    public struct VT
//    {
//        public VT()
//        {
//            this = new VT();
//        }
//    }
    
//    public interface IA
//    {
//        string AA { get; }
//    } 
//    public interface IA<T>
//    {
//        string AA { get; }
//    }
//    public class A : IA
//    {
//        public string AA => "";
//    }
//    public class TestClass//<TKey, TValue>
//    {
//        const int AAA = 1;
//        int x = 0;
//        public static readonly int[] primes = {
//            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
//            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
//            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
//            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
//            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369 };
//        Dictionary<int, int> ix;
//        private static string ToHex(uint x, int precision)
//        {
//            //    var result = dotnetJs.Script.Call<string>("x.toString", 16);
//            //    precision -= result.Length;

//            //    for (var i = 0; i < precision; i++)
//            //    {
//            //        result = "0" + result;
//            //    }

//            //    return result;
//            return "";
//        }
        
//        //private struct Entry
//        //{
//        //    public int hashCode;    // Lower 31 bits of hash code, -1 if unused
//        //    public int next;        // Index of next entry, -1 if last
//        //    public TKey key;           // Key of entry
//        //    public TValue value;         // Value of entry
//        //}

//        //private int[] buckets;
//        //private object simpleBuckets;
//        //private Entry[] entries;
//        //public bool ContainsValue(TValue value)
//        //{
//        //    EqualityComparer<TValue> c = EqualityComparer<TValue>.Default;
//        //    for (int i = 0; i < 10; i++) 
//        //    {
//        //        if (entries[i].hashCode >= 0 && c.Equals(entries[i].value, value)) return true;
//        //    }
//        //    return false;
//        //}
//        string[] abc = new string[] { };
//        IDisposable ds;
//        int abcd;
        
//        void Do2(ref int aa)
//        {
//            var abc = aa.ToString();
//        }
//        void Do(ref int a)
//        {
//            a = 0;
//            Do2(ref a);
//        }
//        private static string ToHex(byte x)
//        {
//            //var result = dotnetJs.Script.Call<string>("x.toString", 16);

//            //if (result.Length == 1)
//            //{
//            //    result = "0" + result;
//            //}

//            //return result;
//            return null;
//        }
//        void TestMethod()
//        {
//            int i = Array.BinarySearch(new int[] { 1 }, 0,
//                                       5, 2, this.As<IComparer<int>>());
//            abcd = 1;
//            var a = DateTime.Now;
//            BitConverter.GetBytes(true);
//            new byte[] { 1 }.Map(ToHex).Join();
//            ds?.Dispose();
//            //var t = this?.As<IDisposable>()?.As<byte[]>()?.Length;
//            ix.GetValueOrDefault(0); 
//            abc.Push("");
//            abc.Some(e => e == "1");
//            Contract.Ensures(Contract.Result<int[]>() != null);
//            Action fallback = () =>
//            {
//                TestMethod();
//            };

//            Func<char, char> swap = ch => (char)(((byte)ch << 8) | (byte)(ch >> 8));

//            Func<char?> readPair = () =>
//            {
//                return '\0';
//            };

//            var firstWord = readPair();
//            ParamMethod("");
//            ParamMethod(1);
//            ParamMethod<string>("a");
//            //var s = ToHex((uint)1, 8) + ToHex((ushort)1, 4) + ToHex((ushort)1, 4);
//            var s = (new[] { (byte)1 }).GetValue(0, 1, 2);//.Join("");
//            var fs = dotnetJs.Script.Write<dynamic>("require({0}, {1}, {2})", 1, 2, 3); //require(1, 2)
//            int Va, b = 1;
//        }

//        void ParamMethod(string a)
//        {
            
//        }

//        void ParamMethod<T>(T a, params string[] b) 
//        {
//            int aa = 0;
//            Do(ref aa);
//            var abc = new Test2() { Property = "" };
//            var abcd = new Test2(1);
//            var aba = new Test2<int>();
//            var abe = new Test2<int>(1);
//        }

//        public static implicit operator Test2(TestClass dateTime)
//        {
//            return new Test2();
//        }
//    }
    
//    public class Test2
//    {
//        public string Property { get; set; }
//        int aa;
//        public Test2() { }
//        public Test2(int a)
//        {
//            aa = a;
//        }
//    }
//    public class Test2<T> : Test2
//    {
//        public string Property { get; set; }
//        public Test2() { }
//        public Test2(int a) : base(a + 20) { }
//    }
//}
