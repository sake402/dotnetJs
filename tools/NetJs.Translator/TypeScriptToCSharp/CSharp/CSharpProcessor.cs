using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;
using TypeScriptToCSharp.CSharp.Lexicon;

namespace TypeScriptToCSharp.CSharp
{
    public class CSharpProcessor
    {
        public CSharpProcessor()
        {
            Namespaces = new List<Namespace>();
            Objects = new List<Lexicon.Type>();
            Lookups = new List<LookupObjectType>();
        }

        List<Lexicon.Type> Objects { get; set; }
        public void Add(Lexicon.Type type)
        {
            Objects.Add(type);
        }
        List<LookupObjectType> Lookups { get; set; }
        public void Add(Namespace type)
        {
            Namespaces.Add(type);
        }
        List<Namespace> Namespaces { get; set; }

        public Lexicon.Type? GetDataTypeByName(string name, Func<string, Lexicon.Type> create)
        {
            var dt = Objects.SingleOrDefault(ddt =>
            {
                return ddt.Name.Equals(name);
            });
            if (dt == null && create != null)
            {
                dt = create(name);
                Objects.Add(dt);
            }
            return dt;
        }

        public Lexicon.Type GetOrCreateDataTypeByName(ICSharpClosure? closure, string name, Func<string, Lexicon.Type>? create = null)
        {
            var dt = closure?.GetByName(name);
            if (dt != null)
                return dt;
            dt = Objects.SingleOrDefault(ddt => ddt.FullName.Equals(name) && (ddt.Closure?.Equals(closure)??closure == ddt.Closure));
            if (dt == null)
            {
                dt = Lookups.FirstOrDefault(ddt => ddt.FullName.Equals(name) && (ddt.FromClosure?.Equals(closure) ?? closure == ddt.FromClosure));
                if (dt == null)
                {
                    if (create == null)
                    {
                        LookupObjectType lt = new LookupObjectType(closure, name);
                        dt = lt;
                        Lookups.Add(lt);
                    }
                    else
                    {
                        Objects.Add(dt = create(name));
                    }
                }
            }
            return dt;
        }

        public TType GetOrCreateDataTypeByName<TType>(ICSharpClosure closure, string name, Func<string, TType>? create = null)
            where TType: Lexicon.Type
        {
            var dt = (TType?)closure?.GetByName(name);
            if (dt != null)
                return dt;
            dt = (TType)Objects.SingleOrDefault(ddt => ddt.FullName.Equals(name) && (ddt.Closure?.Equals(closure) ?? closure == ddt.Closure));
            if (dt == null)
            {
                //dt = Lookups.SingleOrDefault(ddt => ddt.FullName.Equals(name) && (ddt.FromClosure?.Equals(closure) ?? closure == ddt.FromClosure));
                if (dt == null)
                {
                    //if (create == null)
                    //{
                    //    LookupObjectType lt = new LookupObjectType(closure, name);
                    //    dt = lt;
                    //    Lookups.Add(lt);
                    //}
                    //else
                    {
                        Objects.Add(dt = create(name));
                    }
                }
            }
            return dt;
        }

        public Lexicon.Type GetOrCreateArrayDataTypeByName(ICSharpClosure closure, Lexicon.Type type)
        {
            string name =  type.FullName + "[]";
            var dt = closure?.GetByName(name);
            if (dt != null)
                return dt;
            dt = Objects.SingleOrDefault(ddt => ddt.FullName.Equals(name) && (ddt.Closure?.Equals(closure) ?? closure == ddt.Closure));
            if (dt == null)
            {
                dt = Lookups.FirstOrDefault(ddt => ddt.FullName.Equals(name) && (ddt.FromClosure?.Equals(closure) ?? closure == ddt.FromClosure));
                if (dt == null)
                {
                    LookupObjectType lt = new LookupObjectType(closure, name);
                    dt = lt;
                    Lookups.Add(lt);
                }
            }
            return dt;
        }

        public Namespace GetOrCreateNamespaceByName(Namespace? parent, string name)
        {
            var dt = Namespaces.SingleOrDefault(ddt => ddt.Name.Equals(name));
            if (dt == null)
            {
                dt = new Namespace(name, parent);
            }
            return dt;
        }

        public Namespace? GetNamespaceByQualifiedName(string path)
        {
            path = path.FormatCSharpTypeName();
            string[] paths = path.Split(new char[] { '.' });
            Namespace ns = Namespaces.SingleOrDefault(nns => nns.Name.Equals(paths[0]));
            int i = 1;
            if (ns != null)
            {
                for (; i < paths.Length; i++)
                {
                    ns = ns.GetNamespaceByName(paths[i]);
                    if (ns == null)
                        break;
                }
            }
            return ns;
        }

        public Namespace GetOrCreateNamespaceByQualifiedName(string path)
        {
            path = path.FormatCSharpTypeName();
            string[] paths = path.Split(new char[] { '.' });
            Namespace ns = Namespaces.SingleOrDefault(nns => nns.Name.Equals(paths[0]));
            Namespace lastNs = ns;
            int i = 1;
            if (ns != null)
            {
                for (; i < paths.Length; i++)
                {
                    lastNs = ns;
                    ns = ns.GetNamespaceByName(paths[i]);
                    if (ns == null)
                        break;
                }
            }
            else
            {
                lastNs = new Namespace(paths[0], null);
                Namespaces.Add(lastNs);
            }
            ns = lastNs;
            for (; i < paths.Length; i++)
            {
                ns = new Namespace(paths[i], ns);
            }
            return ns;
        }


        public void BuildReference()
        {
            foreach(var look in Lookups)
            {
                var obj = Objects.SingleOrDefault(ob => ob.FullName.Equals(look.Name));
                look.ReferencedObject = obj;
            }
        }

        IEnumerable<Namespace> NamespaceIn(IEnumerable<Namespace> ns)
        {
            IEnumerable<Namespace> childs = ns.SelectMany(nns => nns.Childs);
            if (childs.Count() > 0)
            {
                var inChild = NamespaceIn(childs);
                return childs.Concat(inChild);
            }
            return childs;
        }

        public void ResolveNameConfilict()
        {
            IEnumerable<Namespace> namespaces = Namespaces.Concat(NamespaceIn(Namespaces));
            IEnumerable<INamedConstruct> inamespaces = namespaces;
            IEnumerable<INamedConstruct> syntaxes = inamespaces.Concat(namespaces.SelectMany(nm => nm.Objects));
            var group = syntaxes.GroupBy(sy => sy.FullName);
            var keys = group.Select(gr => gr.Key);
            var keyValue = group.ToDictionary(g => g.Key, g => g);
            var conflicts = keyValue.Where(kv => kv.Value.Count() > 1);
            if (conflicts.Count() > 1)
            {
                Console.WriteLine($"Found {conflicts.Count()} conflicting names");
                foreach (var conflict in conflicts)
                {
                    var objs = conflict.Value;
                    int ix = 1;
                    //List<Class> classes = new List<Class>();
                    //foreach (var obj in objs)
                    //{
                    //    if ((obj is Class @class))
                    //    {
                    //        if (!@class.Partial)
                    //        {
                    //            classes.Add(@class);
                    //            break;
                    //        }
                    //    }
                    //}
                    foreach (var obj in objs)
                    {
                        if ((obj is Class @class))
                        {
                            @class.Rename(@class.Name + ix);
                            ix++;
                            break;
                        }
                    }
                }
            }
        }


        public void ImplementInterfaces()
        {
            foreach (var @namespace in Namespaces)
            {
                foreach (var obj in @namespace.Objects.ToList())
                {
                    Interface? @class = obj.UnderlyingType as Interface;
                    if (@class != null)
                    {
                        ConcreteClass? cClass = @namespace.GetByName(@class.Name + "Class") as ConcreteClass;
                        cClass = cClass ?? new ConcreteClass(@namespace, @class.Name + "Class");
                        cClass.SetAttributes(new List<string>() { "ObjectLiteral" });
                        cClass.DisableBaseClassImplementation = true;
                        cClass.DisableExplicitMemberImplementation = true;
                        cClass.RewriteConflictingFieldName = true;
                        cClass.Implements.Add(@class);
                    }
                }
            }
        }

        void CreatePOCOClass(Namespace @namespace, Class @class)
        {
            List<ICSharpSyntax> generatedFields = new List<ICSharpSyntax>();
            List<string> generatedNames = new List<string>();
            var properties = @class.GetProperties().SelectMany(p => p.Value).ToList();
            int ix = 1;
            var newProperties = properties.ConvertAll(m =>
            {
                if (generatedFields.Any(f => f.Equals(m)))
                    return null;
                string originalName = m.Name;
                string name = m.Name;
                bool nameChanged = false;
                //if (generatedNames.Contains(name))
                //{
                //    name += ix++;
                //    nameChanged = true;
                //}
                Property p = new Property(@class, name, m.Access, m.Type, m.Static&false, false, false);
                p.Comment = m.Comment;
                generatedFields.Add(m);
                generatedNames.Add(name);
                if (nameChanged)
                {
                    p.Attributes.Add($"Name(\"{originalName}\")");
                }
                return p;
            }).Where(mm => mm != null);
            var methods = @class.GetMethods().SelectMany(p => p.Value).Where(m => !m.IsConstructor).ToList();
            var newMethods = methods.ConvertAll(m =>
            {
                if (generatedFields.Any(f => f.Equals(m)))
                    return null;
                string originalName = m.Name;
                string name = m.Name;
                bool nameChanged = false;
                //if (generatedNames.Contains(name))
                //{
                //    name += ix++;
                //    nameChanged = true;
                //}
                Property p = new Property(@class, name, m.Access, new MethodDelegate(null, name, m.Return, m.Parameters), m.Static&false, false, false);
                p.Comment = m.Comment;
                generatedFields.Add(m);
                generatedNames.Add(name);
                if (nameChanged) {
                    p.Attributes.Add($"Name(\"{originalName}\")");
                }
                return p;
            }).Where(mm => mm != null);
            var constructors = @class.GetMethods().SelectMany(p => p.Value).Where(m => m.IsConstructor);
            ConcreteClass? newClass = @namespace.GetByName(@class.Name + "ConfigClass") as ConcreteClass;
            newClass = newClass ?? new ConcreteClass(@namespace, @class.Name + "ConfigClass");
            newClass.SetAttributes(new List<string>() { "ObjectLiteral" });
            List<Method> generatedConstructors = new List<Method>();
            foreach(var constructor in constructors)
            {
                if (!generatedConstructors.Any(c => c.Equals(constructor)))
                {
                    newClass.Properties.Add(new Property(@class, "constructor", AccessSpecifier.Public, new MethodDelegate(null, constructor.Name, new Lexicon.Type(null, "void"), constructor.Parameters)));
                    generatedConstructors.Add(constructor);
                }
            }
            newClass.Properties.AddRange(newProperties!);
            newClass.Properties.AddRange(newMethods!);
        }

        void CreatePOCOClass(Namespace @namespace)
        {
            foreach (var obj in @namespace.Objects.ToList())
            {
                Class? @class = obj.UnderlyingType as Class;
                if (@class != null)
                {
                    if (@class.Name == "Container")
                    {

                    }
                    CreatePOCOClass(@namespace, @class);
                }
            }
            foreach (var innernamespace in @namespace.Childs.ToList())
            {
                CreatePOCOClass(innernamespace);
            }
        }

        public void CreatePOCOClasses()
        {
            foreach(var @namespace in Namespaces)
            {
                CreatePOCOClass(@namespace);
            }
        }

        public string Transpile(string prologue, string epilogue, bool comment)
        {
            StringBuilderFormatter formatter = new StringBuilderFormatter();
            if (prologue != null)
                formatter.WriteLine(prologue);
            foreach (var @namespace in Namespaces)
            {
                @namespace.Write(formatter, comment);
            }
            if (epilogue != null)
                formatter.WriteLine(epilogue);
            return formatter.ToString();
        }
    }
}
