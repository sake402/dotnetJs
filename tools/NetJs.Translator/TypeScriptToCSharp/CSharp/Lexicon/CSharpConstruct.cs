using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class CSharpConstruct : CSharpSyntax, INamedConstruct
    {
        public CSharpConstruct(ICSharpClosure? closure, string name)
        {
            if (typeof(Type).IsAssignableFrom(GetType()))
            {
                Name = name;
            }
            else
            {
                Name = name.FormatCSharpTypeName();
            }
            if (closure == this)
                throw new InvalidOperationException();
            Closure = closure;
            if (Closure != null)
            {
                Closure.Add(this);
            }
        }

        public override void PreConvert() { }

        //Type? defaultValue;
        public virtual Type? DefaultValue { get; set; }
        public ICSharpClosure? Closure { get; protected set; }
        public virtual string Name { get; /*protected */set; }
        public virtual string FullName { get { return Closure != null ? $"{Closure.FullName}.{Name}" : Name; } }

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            formatter.Write($"{Name}");
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
                return Name;
            return base.ToString();
        }

        public override bool Equals(object obj)
        {
            Type? ob = obj as Type;
            if (ob != null && ob != this && ob.GetType() == GetType())
            {
                return FullName.Equals(ob.FullName);
            }
            return base.Equals(obj);
        }
    }
}
