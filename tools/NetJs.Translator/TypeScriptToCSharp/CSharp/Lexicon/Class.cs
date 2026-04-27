using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public abstract class Class : Struct
    {
        static ICSharpClosure GetClosure(ICSharpClosure closure)
        {
            while (closure != null && (closure.ClosureType == ClosureType.Interface || closure.ClosureType == ClosureType.AnonymousInterface))
            {
                closure = closure.Parent;
            }
            return closure;
        }

        public Class(ICSharpClosure? closure, string name, AccessSpecifier access = AccessSpecifier.Public, bool @partial = true, IEnumerable<Class>? baseClass = null) : base(GetClosure(closure), name, access, @partial)
        {
            Implements = new List<Type>();
            Extends = new List<Type>();
            if (baseClass != null)
            {
                Extends.AddRange(baseClass);
            }
            Attributes = new List<string>();
        }

        // public override string Designator => Methods.Count > 0 ? "interface" : "class";
        public List<Type> Extends { get; private set; }
        public List<Type> Implements { get; private set; }
        public bool DisableBaseClassImplementation { get; set; }
        public bool DisableInterfaceFieldImplementation { get; set; }
        public bool DisableExplicitMemberImplementation { get; set; }
        public bool RewriteConflictingFieldName { get; set; }

        public override ClosureType ClosureType =>ClosureType.Class;

        public override bool InheritFrom(Object @object)
        {
            var baseClass = GetBaseClass();
            return baseClass.Any(cl => cl.Equals(@object));
        }

        public void SetAttributes(IEnumerable<string> _attributes)
        {
            Attributes.Clear();
            Attributes.AddRange(_attributes);
        }

        public override string FullName
        {
            get
            {
                return ((Closure != null && Closure.FullName != "Global") ? Closure.FullName + "." : "") + Name;
            }
        }

        public List<string> Attributes { get; protected set; }

        public void Rename(string newName)
        {
            Method constructor = Methods.SingleOrDefault(im => im.Name == Name);
            if (constructor != null)
            {
                constructor.Name = newName;
            }
            Attributes.Add($"Name(\"{Name}\")");
            Name = newName;
        }

        protected override void PreObjectWrite(ICSharpFormatter formatter)
        {
            foreach (var attr in Attributes)
            {
                formatter.WriteLine($"[{attr}]");
            }
        }

        protected override void WriteObjectConstraint(ICSharpFormatter formatter)
        {
            string? baseClasses = null;

            if (!DisableBaseClassImplementation)
            {
                baseClasses = string.Join(", ", Extends.Concat(Implements).ToList().ConvertAll(cls => cls.Name));
                if (!string.IsNullOrEmpty(baseClasses))
                {
                    baseClasses = " : " + baseClasses;
                }
                formatter.Write(baseClasses);
            }
            base.WriteObjectConstraint(formatter);
        }

        Dictionary<string, int>? FieldRewriteSeed;

        protected override void WriteObjectFields(ICSharpFormatter formatter, bool comment)
        {
            if (ClosureType != ClosureType.Interface && !DisableInterfaceFieldImplementation)
            {
                FieldRewriteSeed = new Dictionary<string, int>();
                ImplementInterface(formatter, comment);
            }
        }

        //public override void Write(ICSharpFormatter formatter, bool comment = true)
        //{
        //    if (comment)
        //    {
        //        if (Comment != null)
        //        {
        //            Comment.Write(formatter);
        //        }
        //    }
        //    string baseClasses = null;

        //    if (!DisableBaseClassImplementation)
        //    {
        //        baseClasses = string.Join(", ", Extends.Concat(Implements).ToList().ConvertAll(cls => cls.FullName));
        //        if (!string.IsNullOrEmpty(baseClasses))
        //        {
        //            baseClasses = " : " + baseClasses;
        //        }
        //    }
        //    foreach(var attr in Attributes)
        //    {
        //        formatter.WriteLine($"[{attr}]");
        //    }
        //    formatter.Write($"{(Access != AccessSpecifier.Private ? Access.ToString().ToLower() : "")} {(Partial ? "partial " : "")}{Designator ?? ObjectType.ToString().ToLower()} {Name}{baseClasses}");
        //    formatter.WriteLine(" {");
        //    foreach (var m in InnerObjects.Distinct())
        //    {
        //        m.Write(formatter, comment);
        //    }
        //    List<ICSharpSyntax> generatedFields = new List<ICSharpSyntax>();
        //    List<string> generatedNames = new List<string>();
        //    int ix = 1;
        //    foreach (var m in Variables)
        //    {
        //        if (!generatedFields.Any(f => f.Equals(m)))
        //        {
        //            string originalName = m.Name;
        //            string name = m.Name;
        //            if (generatedNames.Contains(name))
        //            {
        //                name += ix++;
        //                m.Attributes.Add($"Name(\"{originalName}\")");
        //                m.Name = name;
        //            }
        //            m.Write(formatter, comment);
        //            generatedNames.Add(name);
        //            generatedFields.Add(m);
        //        }
        //    }
        //    ix = 1;
        //    foreach (var m in Properties)
        //    {
        //        if (!generatedFields.Any(f => f.Equals(m)))
        //        {
        //            string originalName = m.Name;
        //            string name = m.Name;
        //            if (generatedNames.Contains(name))
        //            {
        //                name += ix++;
        //                m.Attributes.Add($"Name(\"{originalName}\")");
        //                m.Name = name;
        //            }
        //            m.Write(formatter, comment);
        //            generatedNames.Add(name);
        //            generatedFields.Add(m);
        //        }
        //    }
        //    ix = 1;
        //    foreach (var m in Methods)
        //    {
        //        if (!generatedFields.Any(f => f.Equals(m)))
        //        {
        //            string originalName = m.Name;
        //            string name = m.Name;
        //            if (generatedNames.Contains(name) && !m.IsConstructor)
        //            {
        //                name += ix++;
        //                m.Attributes.Add($"Name(\"{originalName}\")");
        //                m.Name = name;
        //            }
        //            m.Write(formatter, comment);
        //            generatedNames.Add(name);
        //            generatedFields.Add(m);
        //        }
        //    }
        //    if (ObjectType != ObjectType.Interface && !DisableInterfaceFieldImplementation)
        //    {
        //        FieldRewriteSeed = new Dictionary<string, int>();
        //        ImplementInterface(formatter, comment);
        //        //List<string> implemented = new List<string>();
        //        //AddImplementedProperties(this, implemented);
        //        //AddImplementedMethods(this, implemented);
        //        //AddExtendFields(Extends, implemented);
        //        //foreach (var m in Implements.Distinct())
        //        //{
        //        //    Interface @interface = m.UnderlyingType as Interface;
        //        //    if (@interface != null)
        //        //    {
        //        //        ImplementInterface(formatter, comment, @interface, implemented);
        //        //    }
        //        //}
        //    }
        //    formatter.WriteLine("}");
        //}

        //void AddImplementedProperty(ObjectProperty property, List<string> implemented)
        //{
        //    implemented.Add(property.Type.Name + " " + property.Name);
        //    implemented.Add( property.Name);
        //}

        //void AddImplementedProperties(Class @class, List<string> implemented)
        //{
        //    foreach (var m in @class.Properties)
        //    {
        //        AddImplementedProperty(m, implemented);
        //    }
        //}

        //bool IsImplementedProperty(ObjectProperty property, List<string> implemented)
        //{
        //    return implemented.Contains(property.Type.Name + " " + property.Name);
        //}

        //bool IsImplementedPropertyName(ObjectProperty property, List<string> implemented)
        //{
        //    return implemented.Contains(property.Name);
        //}

        //void AddImplementedMethod(ObjectMethod method, List<string> implemented)
        //{
        //    implemented.Add(method.Return.Name + " " + method.Name);
        //    implemented.Add(method.Name);
        //}

        //void AddImplementedMethods(Class @class, List<string> implemented)
        //{
        //    foreach (var m in @class.Methods)
        //    {
        //        AddImplementedMethod(m, implemented);
        //    }
        //}

        //bool IsImplementedMethod(ObjectMethod method, List<string> implemented)
        //{
        //    return implemented.Contains(method.Return.Name + " " + method.Name);
        //}

        //bool IsImplementedMethodName(ObjectMethod method, List<string> implemented)
        //{
        //    return implemented.Contains(method.Name);
        //}

        //void AddExtendFields(List<Type> extends, List<string> implemented)
        //{
        //    foreach (var extend in extends)
        //    {
        //        Class @class = extend as Class;
        //        if (@class != null)
        //        {
        //            foreach (var m in @class.Methods)
        //            {
        //                AddImplementedMethod(m, implemented);
        //            }
        //            foreach (var m in @class.Properties)
        //            {
        //                AddImplementedProperty(m, implemented);
        //            }
        //            AddExtendFields(@class.Extends, implemented);
        //        }
        //    }

        //}

        string GetFieldRewriteSeed(string fieldName)
        {
            int n = 1;
            if (!FieldRewriteSeed.ContainsKey(fieldName))
            {
                FieldRewriteSeed[fieldName] = 1;
            }
            else
            {
                FieldRewriteSeed[fieldName]++;
                n = FieldRewriteSeed[fieldName];
            }
            return n.ToString();
        }

        protected virtual void GetBaseClass(Class @class, List<Class> classes)
        {
            foreach(var baseClass in @class.Implements)
            {
                if (!classes.Any(c=> c.Equals(baseClass)))
                {
                    if (baseClass.UnderlyingType is Class class2)
                    {
                        classes.Add(class2);
                    }
                }
            }
            foreach (var baseClass in @class.Extends)
            {
                if (!classes.Any(c => c.Equals(baseClass)))
                {
                    if (baseClass.UnderlyingType is Class class2)
                    {
                        classes.Add(class2);
                    }
                }
            }
            foreach (var baseClass in @class.Implements)
            {
                if (baseClass.UnderlyingType is Class class2)
                {
                    GetBaseClass(class2, classes);
                }
            }
            foreach (var baseClass in @class.Extends)
            {
                if (baseClass.UnderlyingType is Class class2)
                {
                    GetBaseClass(class2, classes);
                }
            }
        }

        List<Class> baseClasses;
        public virtual IEnumerable<Class> GetBaseClass()
        {
            if (baseClasses != null)
                return baseClasses;
            baseClasses = new List<Class>();
            GetBaseClass(this, baseClasses);
            return baseClasses;
        }

        protected virtual void GetInterfaces(Class @class, Dictionary<Class, IEnumerable<Interface>> interfaceDictionary)
        {
            List<Interface>? interfaces = null;
            if (interfaceDictionary.ContainsKey(@class))
            {
                interfaces = interfaceDictionary[@class] as List<Interface>;
            }
            else
            {
                interfaces = new List<Interface>();
                interfaceDictionary[@class] = interfaces;
            }
            foreach (var method in @class.Implements)
            {
                if (method.UnderlyingType is Interface @interface)
                {
                    if (!interfaces.Any(m => m.Equals(@interface)))
                    {
                        interfaces.Add(@interface);
                    }
                }
            }
            foreach (var @base in @class.Extends)
            {
                var bClass = @base.UnderlyingType as Class;
                if (bClass != null)
                {
                    GetInterfaces(bClass, interfaceDictionary);
                }
            }
            foreach (var @base in @class.Implements)
            {
                var bClass = @base.UnderlyingType as Class;
                if (bClass != null)
                {
                    GetInterfaces(bClass, interfaceDictionary);
                }
            }
        }

        Dictionary<Class, IEnumerable<Interface>>? interfaces;
        public virtual Dictionary<Class, IEnumerable<Interface>> GetInterfaces()
        {
            if (interfaces != null)
                return interfaces;
            interfaces = new Dictionary<Class, IEnumerable<Interface>>();
            GetInterfaces(this, interfaces);
            return interfaces;
        }

        protected virtual void GetMethods(Class @class, Dictionary<Class, IEnumerable<Method>> methodDictionary)
        {
            List<Method>? methods = null;
            if (methodDictionary.ContainsKey(@class))
            {
                methods = methodDictionary[@class] as List<Method>;
            }
            else
            {
                methods = new List<Method>();
                methodDictionary[@class] = methods;
            }
            foreach (var method in @class.Methods)
            {
                if (!methods.Any(m=> m.Equals(method)))
                {
                    methods.Add(method);
                }
            }
            foreach(var @base in @class.Extends)
            {
                var bClass = @base as Class;
                if (bClass != null)
                {
                    GetMethods(bClass, methodDictionary);
                }
            }
            foreach (var @base in @class.Implements)
            {
                var bClass = @base as Class;
                if (bClass != null)
                {
                    GetMethods(bClass, methodDictionary);
                }
            }
        }

        Dictionary<Class, IEnumerable<Method>>? methods;
        public virtual Dictionary<Class, IEnumerable<Method>> GetMethods()
        {
            if (methods != null)
                return methods;
            methods = new Dictionary<Class, IEnumerable<Method>>();
            GetMethods(this, methods);
            return methods;
        }

        protected virtual void GetProperties(Class @class, Dictionary<Class, IEnumerable<Property>> propertyDictionary)
        {
            List<Property>? properties = null;
            if (propertyDictionary.ContainsKey(@class))
            {
                properties = propertyDictionary[@class] as List<Property>;
            }
            else
            {
                properties = new List<Property>();
                propertyDictionary[@class] = properties;
            }
            foreach (var property in @class.Properties)
            {
                if (!properties.Any(m => m.Equals(property)))
                {
                    properties.Add(property);
                }
            }
            foreach (var @base in @class.Extends)
            {
                var bClass = @base.UnderlyingType as Class;
                if (bClass != null)
                {
                    GetProperties(bClass, propertyDictionary);
                }
            }
            foreach (var @base in @class.Implements)
            {
                var bClass = @base.UnderlyingType as Class;
                if (bClass != null)
                {
                    GetProperties(bClass, propertyDictionary);
                }
            }
        }

        Dictionary<Class, IEnumerable<Property>>? properties;
        public virtual Dictionary<Class, IEnumerable<Property>> GetProperties()
        {
            if (properties != null)
                return properties;
            properties = new Dictionary<Class, IEnumerable<Property>>();
            GetProperties(this, properties);
            return properties;
        }

        protected virtual void GetVariables(Class @class, Dictionary<Class, IEnumerable<Variable>> variableDictionary)
        {
            List<Variable>? variables = null;
            if (variableDictionary.ContainsKey(@class))
            {
                variables = variableDictionary[@class] as List<Variable>;
            }
            else
            {
                variables = new List<Variable>();
                variableDictionary[@class] = variables;
            }
            foreach (var variable in @class.Variables)
            {
                if (!variables.Any(m => m.Equals(variable)))
                {
                    variables.Add(variable);
                }
            }
            foreach (var @base in @class.Extends)
            {
                var bClass = @base as Class;
                if (bClass != null)
                {
                    GetVariables(bClass, variableDictionary);
                }
            }
            foreach (var @base in @class.Implements)
            {
                var bClass = @base as Class;
                if (bClass != null)
                {
                    GetVariables(bClass, variableDictionary);
                }
            }
        }

        Dictionary<Class, IEnumerable<Variable>>? variables;
        public virtual Dictionary<Class, IEnumerable<Variable>> GetVariables()
        {
            if (variables != null)
                return variables;
            variables = new Dictionary<Class, IEnumerable<Variable>>();
            GetVariables(this, variables);
            return variables;
        }

        void Implement(ICSharpFormatter formatter, Interface @interface, Property property, bool comment, bool @explicit = false, string rewritePropertyName = null)
        {
            if (comment)
            {
                property?.Comment.Write(formatter);
            }
            if (!@explicit)
            {
                formatter.WriteLine($"public extern {property.Type} {property.Name} {{ get; set; }}");
            }
            else
            {
                if (rewritePropertyName != null)
                {
                    formatter.WriteLine($"[Name(\"{property.Name}\")]");
                }
                rewritePropertyName = rewritePropertyName ?? $"{@interface.FullName}.{property.Name}";
                formatter.WriteLine($"extern {property.Type} {rewritePropertyName} {{ get; set; }}");
            }
        }

        void Implement(ICSharpFormatter formatter, Interface @interface, Method method, bool comment, bool @explicit = false, string rewriteMethodName = null)
        {
            if (comment)
            {
                method?.Comment.Write(formatter);
            }
            string @params = string.Join(", ", method.Parameters.ConvertAll(p => p.ToString()));
            if (!@explicit)
            {
                formatter.WriteLine($"public virtual {method.Return} {method.Name}({@params}){(method.Return.Name != "void" ? $" => default({method.Return});" : "{{ }}")}");
            }
            else
            {
                if (rewriteMethodName != null)
                {
                    formatter.WriteLine($"[Name(\"{method.Name}\")]");
                }
                rewriteMethodName = rewriteMethodName ?? $"{@interface.FullName}.{method.Name}";
                formatter.WriteLine($"{method.Return} {rewriteMethodName}({@params}){(method.Return.Name != "void" ? $" => default({method.Return});" : "{{ }}")}");
            }
        }


        void ImplementInterface(ICSharpFormatter formatter, bool comment)
        {
            if (Name == "ComponentConfigClass")
            {

            }

            this.interfaces = null;
            this.properties = null;
            this.methods = null;
            var interfaces = GetInterfaces();
            var properties = GetProperties();
            var methods = GetMethods();
            var directInterfaces = interfaces.Where(m => m.Key.Equals(this)).SelectMany(m => m.Value).ToList();
            var allInterfaces = directInterfaces.Concat(directInterfaces.SelectMany(it => it.GetInterfaces().SelectMany(itt => itt.Value))).ToList();

            {
                Dictionary<Interface, List<Property>> explicitlyImplementedProperties = new Dictionary<Interface, List<Property>>();
                List<Property> implementedProperties = new List<Property>();
                implementedProperties.AddRange(Properties);
                List<string> implementedPropertyNames = new List<string>();
                implementedPropertyNames.AddRange(Properties.Select(m => m.Name));
                List<string> implementedExplicitInterfaceProperties = new List<string>();
                var categorizedProperties = properties.Where(m => m.Key is Interface && allInterfaces.Any(mm=> mm.Equals(m.Key)));
                foreach (var cp in categorizedProperties)
                {
                    Interface @interface = cp.Key as Interface;
                    IEnumerable<Property> interfaceProperties = cp.Value;
                    if (!explicitlyImplementedProperties.ContainsKey(@interface))
                    {
                        explicitlyImplementedProperties[@interface] = new List<Property>();
                    }
                    foreach (var property in interfaceProperties)
                    {
                        if (!implementedProperties.Any(m => m.Equals(property)))
                        {
                            bool explicitImpement = implementedPropertyNames.Any(nm => property.Name.Equals(nm));
                            string rewritePropertyName = null;
                            if (explicitImpement)
                            {
                                if (explicitlyImplementedProperties[@interface].Any(p => p.Equals(property)))
                                    continue;
                                if (implementedExplicitInterfaceProperties.Contains($"{@interface.FullName}.{property.Name}"))
                                {
                                    rewritePropertyName = $"{property.Name}{GetFieldRewriteSeed(property.Name)}";
                                }
                            }
                            if (explicitImpement && DisableBaseClassImplementation) //skip explicit implementation
                                continue;
                            Implement(formatter, @interface, property, comment, explicitImpement, rewritePropertyName);
                            if (explicitImpement)
                            {
                                explicitlyImplementedProperties[@interface].Add(property);
                            }
                            else
                            {
                                implementedProperties.Add(property);
                            }
                            if (rewritePropertyName != null)
                            {
                                implementedPropertyNames.Add(rewritePropertyName);
                            }
                            else
                            {
                                implementedPropertyNames.Add(property.Name);
                            }
                            if (explicitImpement)
                            {
                                implementedExplicitInterfaceProperties.Add($"{@interface.FullName}.{property.Name}");
                                if (rewritePropertyName != null)
                                {
                                    implementedExplicitInterfaceProperties.Add(rewritePropertyName);
                                }
                            }
                        }
                    }
                }
            }

            {
                Dictionary<Interface, List<Method>> explicitlyImplementedMethods = new Dictionary<Interface, List<Method>>();
                List<Method> implementedMethods = new List<Method>();
                implementedMethods.AddRange(Methods);
                List<string> implementedMethodNames = new List<string>();
                implementedMethodNames.AddRange(Methods.Select(m => m.Name));
                List<string> implementedExplicitInterfaceMethods = new List<string>();
                var categorizedMethods = methods.Where(m => m.Key is Interface && allInterfaces.Any(mm => mm.Equals(m.Key)));
                foreach (var cp in categorizedMethods)
                {
                    Interface @interface = cp.Key as Interface;
                    IEnumerable<Method> interfaceMethods = cp.Value;
                    if (!explicitlyImplementedMethods.ContainsKey(@interface))
                    {
                        explicitlyImplementedMethods[@interface] = new List<Method>();
                    }
                    foreach (var method in interfaceMethods)
                    {
                        if (!implementedMethods.Any(m => m.Equals(method)))
                        {
                            bool explicitImpement = implementedMethodNames.Any(nm => method.Name.Equals(nm));
                            string rewriteMethodName = null;
                            if (explicitImpement)
                            {
                                if (explicitlyImplementedMethods[@interface].Any(p => p.Equals(method)))
                                    continue;
                                if (implementedExplicitInterfaceMethods.Contains($"{@interface.FullName}.{method.Name}"))
                                {
                                    rewriteMethodName = $"{method.Name}{GetFieldRewriteSeed(method.Name)}";
                                }
                            }
                            if (explicitImpement && DisableBaseClassImplementation) //skip explicit implementation
                                continue;
                            Implement(formatter, @interface, method, comment, explicitImpement, rewriteMethodName);
                            if (explicitImpement)
                            {
                                explicitlyImplementedMethods[@interface].Add(method);
                            }
                            else
                            {
                                implementedMethods.Add(method);
                            }
                            if (rewriteMethodName != null)
                            {
                                implementedMethodNames.Add(rewriteMethodName);
                            }
                            else
                            {
                                implementedMethodNames.Add(method.Name);
                            }
                            if (explicitImpement)
                            {
                                implementedExplicitInterfaceMethods.Add($"{@interface.FullName}.{method.Name}");
                                if (rewriteMethodName != null)
                                {
                                    implementedExplicitInterfaceMethods.Add(rewriteMethodName);
                                }
                            }
                        }
                    }
                }
            }
        }

        //void ImplementInterface(ICSharpFormatter formatter, bool comment, Interface @interface, List<string> implemented = null)
        //{
        //    if (implemented == null)
        //    {
        //        implemented = new List<string>();
        //    }
        //    foreach (var property in @interface.Properties)
        //    {
        //        if (property.Static)
        //            continue;
        //        if (!IsImplementedProperty(property, implemented))
        //        {
        //            if (!IsImplementedPropertyName(property, implemented))
        //            {
        //                if (comment)
        //                {
        //                    property.Comment?.Write(formatter);
        //                }
        //                formatter.WriteLine($"public extern {property.Type} {property.Name} {{ get; set; }}");
        //                AddImplementedProperty(property, implemented);
        //            }
        //            else if (RewriteConflictingFieldName)
        //            {
        //                if (comment)
        //                {
        //                    property.Comment?.Write(formatter);
        //                }
        //                formatter.WriteLine($"[Name(\"{property.Name}\")]");
        //                formatter.WriteLine($"public extern {property.Type} {property.Name}{GetFieldRewriteSeed(property.Name)} {{ get; set; }}");
        //            }
        //            else if (!DisableExplicitMemberImplementation)
        //            {
        //                if (comment)
        //                {
        //                    property.Comment?.Write(formatter);
        //                }
        //                formatter.WriteLine($"extern {property.Type} {@interface.FullName}.{property.Name} {{ get; set; }}");
        //            }
        //        }
        //    }
        //    foreach (var method in @interface.Methods)
        //    {
        //        if (method.Static)
        //            continue;
        //        string @params = string.Join(", ", method.Parameters.ConvertAll(p => p.ToString()));
        //        if (!IsImplementedMethod(method, implemented))
        //        {
        //            if (!IsImplementedMethodName(method, implemented))
        //            {
        //                if (comment)
        //                {
        //                    method.Comment?.Write(formatter);
        //                }
        //                formatter.WriteLine($"public extern {method.Return} {method.Name}({@params})");
        //                AddImplementedMethod(method, implemented);
        //            }
        //            else if (RewriteConflictingFieldName)
        //            {
        //                if (comment)
        //                {
        //                    method.Comment?.Write(formatter);
        //                }
        //                formatter.WriteLine($"[Name(\"{method.Name}\")]");
        //                formatter.WriteLine($"public extern {method.Return} {method.Name}{GetFieldRewriteSeed(method.Name)}({@params})");
        //            }
        //            else if (!DisableExplicitMemberImplementation)
        //            {
        //                if (comment)
        //                {
        //                    method.Comment?.Write(formatter);
        //                }
        //                formatter.WriteLine($"extern {method.Return} {@interface.FullName}.{method.Name}({@params})");
        //            }
        //        }
        //    }
        //    foreach (var impl in @interface.Implements)
        //    {
        //        Interface @interfac = impl.UnderlyingType as Interface;
        //        if (@interfac != null)
        //        {
        //            ImplementInterface(formatter, comment, interfac, implemented);
        //        }
        //    }
        //}
    }
}
