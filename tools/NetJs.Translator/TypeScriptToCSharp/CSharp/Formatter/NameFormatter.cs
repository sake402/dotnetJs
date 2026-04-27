using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptToCSharp.CSharp.Formatter
{
    public static class NameFormatter
    {
        static readonly string[] disallowedString =
{
            "abstract",
            "as",
            "base",
            "bool",
            "break",
            "byte",
            "case",
            "catch",
            "char",
            "checked",
            "class",
            "const",
            "continue",
            "decimal",
            "default",
            "delegate",
            "do",
            "double",
            "else",
            "enum",
            "event",
            "explicit",
            "extern",
            "false",
            "finally",
            "fixed",
            "float",
            "for",
            "foreach",
            "goto",
            "if",
            "implicit",
            "in",
            "int",
            "interface",
            "internal",
            "is",
            "lock",
            "long",
            "namespace",
            "new",
            "null",
            "object",
            "operator",
            "out",
            "override",
            "params",
            "private",
            "protected",
            "public",
            "readonly",
            "ref",
            "return",
            "sbyte",
            "sealed",
            "short",
            "sizeof",
            "stackalloc",
            "static",
            "string",
            "struct",
            "switch",
            "this",
            "throw",
            "true",
            "try",
            "typeof",
            "uint",
            "ulong",
            "unchecked",
            "unsafe",
            "ushort",
            "using",
            "virtual",
            "volatile",
            "void",
            "while",
            "params",
            "base"
        };
        const string allowedCSChars = "@abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_1234567890";

        public static string ConvertToCSValidName(string value, out bool nameAttribute, string emptyString = "")
        {
            string org = value;
            if (value == "item")
                value = "_item";
            if (value?.Length != 0)
                if (char.IsNumber(value[0]))
                    value = '_' + value;
            if (disallowedString.Contains(value))
                value = $"@{value}";
            char[] @new = value.ToCharArray();
            for (int n = 0; n < value.Length; n++)
                if (!allowedCSChars.Contains(value[n]))
                    @new[n] = '_';
            if (string.IsNullOrEmpty(value))
                value = emptyString;
            if (value == org)
                value = new string(@new);
            nameAttribute = org != value;
            return value;
        }

        public static string FormatCSharpTypeName(this string typeName)
        {
            string[] names = typeName.Split(new char[] { '.', '<', '>', ',', '[', ']', ' ' }).Where(n=> !string.IsNullOrEmpty(n)).ToArray();
            //if (names.Length == 1)
            //    return typeName;
            string[] newNames = new string[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                bool attr;
                if (!string.IsNullOrEmpty(names[i]))
                    newNames[i] = ConvertToCSValidName(names[i], out attr);
            }
            string newName = typeName;
            for (int i = 0; i < names.Length; i++)
            {
                if (!string.IsNullOrEmpty(names[i]) && names[i] != newNames[i])
                    newName = newName.Replace(names[i], newNames[i]);
            }
            return newName;
        }

        public static string FormatCSharpName(this string name)
        {
            bool attr;
            return ConvertToCSValidName(name, out attr);
        }
    }
}
