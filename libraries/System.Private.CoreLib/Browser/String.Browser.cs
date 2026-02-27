using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace System
{
    [dotnetJs.StaticCallConvention]
    public partial class String
    {
        [dotnetJs.Template("{this}.indexOf({value})")]
        public extern int NativeIndexOf(string value);
        [dotnetJs.Template("{this}.indexOf({value}, {start})")]
        public extern int NativeIndexOf(string value, int start);
        [dotnetJs.Template("{this}.replace({pattern}, {value})")]
        public extern string NativeReplace(dotnetJs.Union<string, RegExp> pattern, string value);
        [dotnetJs.Template("{this}.replaceAll({pattern}, {value})")]
        public extern string NativeReplaceAll(dotnetJs.Union<string, RegExp> pattern, string value);
        [dotnetJs.Template("{this}.split({separator})")]
        public extern string[] NativeSplit(dotnetJs.Union<string, RegExp> separator);
        [dotnetJs.Template("{this}.endsWith({pattern})")]
        public extern bool NativeEndsWith(string pattern);

        [dotnetJs.Template("String.fromCharCode({code})")]
        public static extern string NativeFromCharCode(int code);
        [dotnetJs.Template("String.fromCharCode({code1}, {code2})")]
        public static extern string NativeFromCharCode(int code1, int code2);
        [dotnetJs.Template("String.fromCharCode.apply(null, {code})")]
        public static extern string NativeFromCharCode(int[] codes);
        [dotnetJs.Template("String.fromCharCode.apply(null, {code})")]
        public static extern string NativeFromCharCode(char[] codes);
        [dotnetJs.Template("{this}.charCodeAt({i})")]
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
        
        [dotnetJs.MemberReplace(nameof(Length))]
        [dotnetJs.StaticCallConvention(false)]
        [dotnetJs.Name("length")]
        public extern int IntrinsicLength
        {
            [dotnetJs.Template("{this}.length")]
            get;
        }

        [dotnetJs.MemberReplace(nameof(FastAllocateString))]
        internal static string FastAllocateStringImpl(int length)
        {
            return new char[length].As<string>();
        }

        [dotnetJs.MemberReplace(nameof(InternalIsInterned))]
        private static string InternalIsInternedImpl(string str)
        {
            return str;
        }

        [dotnetJs.MemberReplace(nameof(InternalIntern))]
        private static string InternalInternImpl(string str)
        {
            return str;
        }

    }
}
