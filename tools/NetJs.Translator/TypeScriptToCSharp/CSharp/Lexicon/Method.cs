using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class Method : Member, ICSharpClosure
    {
        public Method(ICSharpClosure? @object, string name, AccessSpecifier access, Type? @return, bool @static = false, IEnumerable<MethodParameter>? parameters = null) : base(@object, name, access, @static)
        {
            Return = @return;
            Parameters = new List<MethodParameter>();
            if (parameters != null)
            {
                Parameters.AddRange(parameters);
            }
            IsConstructor = name == @object?.Name.Split(new char[] { '<' })[0];
            IsIndexer = name == null;
        }

        public List<GenericType>? GenericArgs { get; set; }
        public string? Body { get; set; }
        public bool IsConstructor { get; set; }
        public bool IsIndexer { get; set; }
        public bool IsImplicitConverter => Return is ImplicitType;
        //public Interface FromInterface { get; set; }
        public Type? Return { get; set; }

        public List<MethodParameter> Parameters { get; set; } = new List<MethodParameter>();

        public ClosureType ClosureType => ClosureType.Method;

        public ICSharpClosure? Parent => Closure;

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
            string @params = string.Join(", ", Parameters.ConvertAll(p => p.ToString()));
            string @paramsName = string.Join(", ", Parameters.ConvertAll(p => p.Name));
            foreach (var attr in Attributes)
            {
                formatter.WriteLine($"[{attr}]");
            }
            if (!IsIndexer)
            {
                string generics = "";
                if (GenericArgs != null && GenericArgs.Count > 0)
                {
                    generics = "<" + string.Join(", ", GenericArgs.ConvertAll(a =>
                    {
                        var v = a.ToString(); if (v == "void") { v = "undefined"; }
                        return v;
                    })) + ">";
                }
                var name = Name.Split(new char[] { '<' })[0];
                formatter.WriteLine($"{((Closure.ClosureType != ClosureType.Interface && Closure.ClosureType != ClosureType.AnonymousInterface && Access != AccessSpecifier.Private) ? Access.ToString().ToLower() + " " : "")}{(Closure.ClosureType != ClosureType.Interface && Closure.ClosureType != ClosureType.AnonymousInterface &&!IsImplicitConverter && !IsConstructor ? /*"extern "*/"virtual " : "")}{(IsImplicitConverter ? "static " : "")}{(Abstract ? "abstract " : "")}{(Return != null ? Return.ToString() + " " : "")}{name.FormatCSharpName()}{generics}({@params}){( Body != null ? Body : Closure.ClosureType != ClosureType.Interface && Closure.ClosureType != ClosureType.AnonymousInterface && !IsImplicitConverter ? ((Return?.Name??"void") != "void" && !IsConstructor ? $" => default({Return});" : ((IsConstructor && Closure is Class && (Closure as Class).Extends.Any(m=> (m is Class) && (m as Class).Methods.Any(mm => mm.IsConstructor)) && false ? $" : base({@paramsName})" : "") + "{ }")) : ";")}");
            }
            else
            {
                formatter.WriteLine($"{((Closure.ClosureType != ClosureType.Interface && Closure.ClosureType != ClosureType.AnonymousInterface && Access != AccessSpecifier.Private) ? Access.ToString().ToLower() + " " : "")}{(Closure.ClosureType != ClosureType.Interface && Closure.ClosureType != ClosureType.AnonymousInterface ? /*"extern "*/"virtual " : "")}{(Static ? /*"static "*/"" : "")}{(Abstract ? "abstract " : "")}{Return} this[{@params}]{{ get; set; }}");
            }
        }

        public override bool Equals(object obj)
        {
            Method? method = obj as Method;
            if (method != null && method != this)
            {
                if (Parameters.Count == method.Parameters.Count)
                {
                    for (int i = 0; i < method.Parameters.Count; i++)
                    {
                        if (!Parameters[i].Equals(method.Parameters[i]))
                        {
                            return false;
                        }
                    }
                }
                else
                    return false;
                if (Closure == method.Closure || Closure.Equals(method.Closure)) {
                    Object? closure1 = Closure as Object;
                    Object? closure2 = method.Closure as Object;
                    if (closure1 != null && closure2 != null && (closure1.InheritFrom(closure2) || closure2.InheritFrom(closure1)))
                    {
                        if ((method.IsConstructor && IsConstructor) || method.Name.Equals(Name))
                        {
                            if (method.Return.Equals(Return))
                            {
                                if (method.Parameters.Count == Parameters.Count)
                                {
                                    bool match = true;
                                    for (int ix = 0; ix < method.Parameters.Count; ix++)
                                    {
                                        if (!method.Parameters[ix].Equals(Parameters[ix]))
                                        {
                                            match = false;
                                            break;
                                        }
                                    }
                                    return match;
                                }
                            }
                        }
                    }
                }
            }
            return base.Equals(obj);
        }

        public void Add(CSharpConstruct construct)
        {
            if (construct is MethodParameter param)
            {
                Parameters.Add(param);
            }
            else
            {
                Closure?.Add(construct);
            }
        }

        public void Remove(CSharpConstruct construct)
        {
            if (construct is MethodParameter param)
            {
                Parameters.Remove(param);
            }
        }

        public Type? GetByName(string name)
        {
            return null;// Parameters.SingleOrDefault(p=> name.Equals(p.FullName));
        }
    }
}
