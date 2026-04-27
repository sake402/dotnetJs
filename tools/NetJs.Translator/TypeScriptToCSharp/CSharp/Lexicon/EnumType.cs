using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class EnumValue
    {
        public EnumValue(object value)
        {
            Value = value;
        }

        public object Value { get; private set; }
        public Comment Comment { get; set; }

        public override string ToString()
        {
            return $"{Value}";
        }
    }

    public class EnumType : Type
    {
        public EnumType(ICSharpClosure context, string name, AccessSpecifier access = AccessSpecifier.Private) : base(context, name)
        {
            Access = access;
            Values = new Dictionary<string, EnumValue>();
        }

        public AccessSpecifier Access { get; private set; }
        public Dictionary<string, EnumValue> Values { get; private set; }
        public Comment Comment { get; set; }

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            if (comment)
            {
                Comment?.Write(formatter);
            }
            string @enum = string.Join(",\r\n", Values.Select(v => $"{v.Key} = {v.Value}"));
            formatter.WriteLine($"{(Access != AccessSpecifier.Private ? Access.ToString().ToLower() + " " : "")} enum {Name}");
            formatter.WriteLine($"{{");
            int l = Values.Count;
            foreach (var v in Values)
            {
                l--;
                if (v.Value.Value != null)
                {
                    formatter.WriteLine($"{v.Key} = {v.Value}{(l != 0 ? "," : "")}");
                }
                else
                {
                    formatter.WriteLine($"{v.Key}{(l != 0 ? "," : "")}");
                }
            }
            //formatter.WriteLine(@enum);
            formatter.WriteLine($"}}");
        }
    }
}
