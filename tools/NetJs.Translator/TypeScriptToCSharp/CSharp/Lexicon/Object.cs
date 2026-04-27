using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public abstract class Object : Type, ICSharpClosure
    {
        public Object(ICSharpClosure closure, string name, AccessSpecifier access = AccessSpecifier.Private, bool partial = true) : base(closure, name)
        {
            Access = access;
            Partial = partial;
            InnerObjects = new List<Object>();
            Variables = new List<Variable>();
            Methods = new List<Method>();
            Properties = new List<Property>();
        }

        public bool Partial { get; private set; }
        public AccessSpecifier Access { get; set; }
        public Comment Comment { get; set; }
        public List<Variable> Variables { get; set; }
        public List<Method> Methods { get; set; }
        public List<Property> Properties { get; set; }
        public List<Object> InnerObjects { get; private set; }
        public ICSharpClosure Parent => Closure;
        public virtual string Designator => null;

        public abstract ClosureType ClosureType { get; }

        public abstract bool InheritFrom(Object @object);

        public override void PreConvert()
        {
            foreach (var t in Properties)
            {
                t.PreConvert();
            }
            foreach (var t in Methods)
            {
                t.PreConvert();
            }
            foreach (var t in InnerObjects)
            {
                t.PreConvert();
            }
            base.PreConvert();
        }

        public virtual void Add(CSharpConstruct construct)
        {
            if (construct == this)
                throw new InvalidOperationException();
            if (construct is Property property)
            {
                if (!Properties.Contains(property))
                    Properties.Add(property);
            }
            if (construct is Method method)
            {
                if (!Methods.Contains(method))
                    Methods.Add(method);
            }
            if (construct is Object @object)
            {
                if (!InnerObjects.Contains(@object))
                    InnerObjects.Add(@object);
            }
        }

        public virtual void Remove(CSharpConstruct construct)
        {
            if (construct == this)
                throw new InvalidOperationException();
            if (construct is Property property)
            {
                Properties.Remove(property);
            }
            if (construct is Method method)
            {
                Methods.Remove(method);
            }
            if (construct is Object @object)
            {
                InnerObjects.Remove(@object);
            }
        }

        public Type GetByName(string name)
        {
            if (name.Equals(Name))
                return this;
            return InnerObjects.SingleOrDefault(cl => cl.Name.Equals(name));
        }

        protected virtual void PreObjectWrite(ICSharpFormatter formatter)
        {

        }

        protected virtual void PostWrite(ICSharpFormatter formatter)
        {

        }

        protected virtual void WriteObjectFields(ICSharpFormatter formatter, bool comment)
        {

        }

        protected virtual void WriteObjectConstraint(ICSharpFormatter formatter)
        {

        }

        public virtual string Definition => $"{(Access != AccessSpecifier.Private ? Access.ToString().ToLower() + " " : "")}{(Partial ? "partial " : "")}{Designator ?? ClosureType.ToString().ToLower()} {Name}";

        public int AutoDefinedTypesCount { get; set; }

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            if (comment)
            {
                if (Comment != null)
                {
                    Comment.Write(formatter);
                }
            }
            PreObjectWrite(formatter);
            formatter.Write(Definition);
            WriteObjectConstraint(formatter);
            formatter.WriteLine("");
            formatter.WriteLine("{");
            foreach (var m in InnerObjects.Distinct())
            {
                m.Write(formatter, comment);
            }
            List<ICSharpSyntax> generatedFields = new List<ICSharpSyntax>();
            List<string> generatedNames = new List<string>();
            int ix = 1;
            foreach (var m in Variables)
            {
                if (!generatedFields.Any(f => f.Equals(m)))
                {
                    string originalName = m.Name;
                    string name = m.Name;
                    if (generatedNames.Contains(name))
                    {
                        //name += ix++;
                        //m.Attributes.Add($"Name(\"{originalName}\")");
                        //m.Name = name;
                    }
                    m.Write(formatter, comment);
                    generatedNames.Add(name);
                    generatedFields.Add(m);
                }
            }
            ix = 1;
            foreach (var property in Properties)
            {
                if (!generatedFields.Any(f => f.Equals(property)))
                {
                    string originalName = property.Name;
                    string name = property.Name;
                    if (generatedNames.Contains(name))
                    {
                        //name += ix++;
                        //property.Attributes.Add($"Name(\"{originalName}\")");
                        //property.Name = name;
                    }
                    property.Write(formatter, comment);
                    generatedNames.Add(name);
                    generatedFields.Add(property);
                }
            }
            ix = 1;
            foreach (var m in Methods)
            {
                if (!generatedFields.Any(f => f.Equals(m)))
                {
                    string originalName = m.Name;
                    string name = m.Name;
                    if (generatedNames.Contains(name) && !m.IsConstructor)
                    {
                        //name += ix++;
                        //m.Attributes.Add($"Name(\"{originalName}\")");
                        //m.Name = name;
                    }
                    m.Write(formatter, comment);
                    generatedNames.Add(name);
                    generatedFields.Add(m);
                }
            }
            WriteObjectFields(formatter, comment);
            formatter.WriteLine("}");
        }
    }
}
