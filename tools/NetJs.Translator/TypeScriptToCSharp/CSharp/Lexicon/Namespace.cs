using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class Namespace : CSharpConstruct, ICSharpClosure, INamedConstruct
    {
        public Namespace(string name, Namespace? parent = null, Comment? comment = null, bool alreadyDefined = false):base(parent, name)
        {
            Name = name.FormatCSharpTypeName();
            Parent = parent;
            if (Parent != null)
            {
                Parent.Add(this);
            }
            Comment = comment;
            Childs = new List<Namespace>();
            Objects = new List<Type>();
            _alreadyDefined = alreadyDefined;
        }

        public override void PreConvert()
        {
            foreach (var t in Childs.ToList())
            {
                t.PreConvert();
            }
            foreach (var t in Objects.ToList())
            {
                t.PreConvert();
            }
            base.PreConvert();
        }

        bool _alreadyDefined;
        public override string FullName
        {
            get
            {
                string pName = "";
                if (Parent != null && Parent.Name != "Global")
                {
                    pName = Parent.FullName + ".";
                }
                pName += Name;
                return pName;
            }
        }

        public Namespace? Parent { get; set; }
        public List<Namespace> Childs { get; private set; }

        public void Add(CSharpConstruct construct)
        {
            if (construct is Namespace @namespace)
            {
                if (!Childs.Contains(@namespace))
                    Childs.Add(@namespace);
            }
            else if (construct is Type @object)
            {
                if (!Objects.Contains(@object))
                    Objects.Add(@object);
            }
        }

        public void Remove(CSharpConstruct construct)
        {
            if (construct is Namespace @namespace)
            {
                Childs.Remove(@namespace);
            }
            else if (construct is Type @object)
            {
                Objects.Remove(@object);
            }
        }
        public void Add (Namespace @namespace)
        {
        }
        public Namespace GetNamespaceByName(string name)
        {
            return Childs.SingleOrDefault(nm => nm.Name.Equals(name));
        }
        public Type GetObjectByName(string name)
        {
            return Objects.SingleOrDefault(nm => nm.Name.Equals(name));
        }
        Class? globalClass;
        public Class GlobalClass
        {
            get
            {
                if (globalClass == null)
                {
                    globalClass = new ConcreteClass(this, "Global", AccessSpecifier.Public);
                }
                return globalClass;
            }
        }
        public List<Type> Objects { get; set; }
        public Comment? Comment { get; private set; }

        ICSharpClosure? ICSharpClosure.Parent => Parent;

        public ClosureType ClosureType => ClosureType.Namespace;

        public int AutoDefinedTypesCount { get; set; }

        public Type GetByName(string name)
        {
            return Objects.SingleOrDefault(ob =>
            {
                if (!(ob is Member))
                {
                    if (ob.Name.Equals(name))
                        return true;
                    if (name.EndsWith(">"))
                    {
                        var mname = name.Split(new char[] { '<' })[0];
                        return ob.Name.EndsWith(">") && ob.Name.StartsWith(mname);
                    }
                }
                return false;
            });
        }

        bool WillConvertToStaticClass()
        {
            if (Objects.Any(o =>
            {
                var b = o is Variable || o is Method;
                return b;
            }))
            {
                return true;
            }
            if (Parent != null && Parent.ClosureType == ClosureType.Namespace)
            {
                return (Parent as Namespace).WillConvertToStaticClass();
            }
            return false;
        }

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            if (_alreadyDefined)
                return;
            //foreach (var @class in Objects.Where(o => {
            //    if (o is AliasType)
            //        return true;
            //    return false;
            //}))
            //{
            //    @class.Write(formatter, comment);
            //}
            if (Comment != null)
            {
                Comment.Write(formatter, comment);
            }
            if (WillConvertToStaticClass())
            {
                formatter.WriteLine($"public class {Name}" + " {");
            }
            else
            {
                formatter.WriteLine($"namespace {Name}" + " {");
            }
            foreach (var @class in Objects.ToList()/*.Where(o=> {
                if (o is AliasType)
                    return false;
                return true;
            })*/)
            {
                @class.Write(formatter, comment);
            }
            foreach (var @namespace in Childs)
            {
                @namespace.Write(formatter, comment);
            }
            formatter.WriteLine("}");
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
