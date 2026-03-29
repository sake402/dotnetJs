using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace System
{
    [NetJs.StaticCallConvention]
    public partial class String
    {
        [NetJs.Template("{this}.indexOf({value})")]
        public extern int NativeIndexOf(string value);
        [NetJs.Template("{this}.indexOf({value}, {start})")]
        public extern int NativeIndexOf(string value, int start);
        [NetJs.Template("{this}.replace({pattern}, {value})")]
        public extern string NativeReplace(NetJs.Union<string, RegExp> pattern, string value);
        [NetJs.Template("{this}.replaceAll({pattern}, {value})")]
        public extern string NativeReplaceAll(NetJs.Union<string, RegExp> pattern, string value);
        [NetJs.Template("{this}.split({separator})")]
        public extern string[] NativeSplit(NetJs.Union<string, RegExp> separator);
        [NetJs.Template("{this}.endsWith({pattern})")]
        public extern bool NativeEndsWith(string pattern);

        [NetJs.Template("String.fromCharCode({code})")]
        public static extern string NativeFromCharCode(int code);
        [NetJs.Template("String.fromCharCode({code1}, {code2})")]
        public static extern string NativeFromCharCode(int code1, int code2);
        [NetJs.Template("String.fromCharCode.apply(null, {code})")]
        public static extern string NativeFromCharCode(int[] codes);
        [NetJs.Template("String.fromCharCode.apply(null, {code})")]
        public static extern string NativeFromCharCode(char[] codes);
        [NetJs.Template("{this}.charCodeAt({i})")]
        public extern char NativeCharCodeAt(int i);
        
        //[dotnetJs.MemberReplace("ctor(char[])")]
        //public static string Create(char[]? value)
        //{
        //    return Ctor(value);
        //}

        //[dotnetJs.MemberReplace("ctor(char[], int, int)")]
        //public static string Create(char[] value, int startIndex, int length)
        //{
        //    return Ctor(value, startIndex, length);
        //}

        //[dotnetJs.MemberReplace("ctor(char*)")]
        //public static unsafe string Create(char* value)
        //{
        //    return Ctor(value);
        //}

        //[dotnetJs.MemberReplace("ctor(char*, int, int)")]
        //public static unsafe string Create(char* value, int startIndex, int length)
        //{
        //    return Ctor(value, startIndex, length);
        //}

        //[dotnetJs.MemberReplace("ctor(sbyte*)")]
        //public static unsafe string Create(sbyte* value)
        //{
        //    return Ctor(value);
        //}

        //[dotnetJs.MemberReplace("ctor(sbyte*, int, int)")]
        //public static unsafe string Create(sbyte* value, int startIndex, int length)
        //{
        //    return Ctor(value, startIndex, length);
        //}

        //[dotnetJs.MemberReplace("ctor(char, int)")]
        //public static string Create(char c, int count)
        //{
        //    return Ctor(c, count);
        //}

        //[dotnetJs.MemberReplace("ctor(ReadOnlySpan<char>)")]
        //public static string Create(ReadOnlySpan<char> value)
        //{
        //    return Ctor(value);
        //}
        
        [NetJs.MemberReplace(nameof(Length))]
        [NetJs.StaticCallConvention(false)]
        [NetJs.Name("length")]
        public extern int IntrinsicLength
        {
            [NetJs.Template("{this}.length")]
            get;
        }

        [NetJs.MemberReplace(nameof(FastAllocateString))]
        internal static string FastAllocateStringImpl(int length)
        {
            return new char[length].As<string>();
        }

        [NetJs.MemberReplace(nameof(InternalIsInterned))]
        private static string InternalIsInternedImpl(string str)
        {
            return str;
        }

        [NetJs.MemberReplace(nameof(InternalIntern))]
        private static string InternalInternImpl(string str)
        {
            return str;
        }

    }
}
