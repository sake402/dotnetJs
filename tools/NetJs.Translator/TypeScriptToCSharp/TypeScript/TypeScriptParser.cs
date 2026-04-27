using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp;
using TypeScriptToCSharp.CSharp.Lexicon;

namespace TypeScriptToCSharp.TypeScript
{
    
    public class TypeScriptParser
    {
        public TypeScriptParser(string typeScript, CSharpProcessor context, TypeScriptParserSettings settings)
        {
            TypeScript = typeScript;
            Context = context;
            Settings = settings;
        }

        CSharpProcessor Context { get; set; }
        TypeScriptParserSettings Settings { get; set; }
        string TypeScript { get; set; }
        int Index { get; set; }
        public char Current => CharAt(Index);
        public char CharAt(int index) => index < 0 || index >= TypeScript.Length ? '\0' : TypeScript[index];
        public void GoForwardOne() => GoForward(1);
        public void GoForward(int length) => Index += length;
        public const string spaceChars = " \t\r\n";
        public const string wordChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_$1234567890.";

        public Stack<int> restorePoints = new Stack<int>();

        public int CreateRestorePoint()
        {
            restorePoints.Push(Index);
            return Index;
        }

        public int Restore()
        {
            int index = Index;
            Index = restorePoints.Pop();
            return index;
        }

        private void SkipUntil(char value)
        {
            while (!(Current == value)) GoForwardOne();
            GoForwardOne();
        }

        private void SkipUntil(string value)
        {
            while (!CurrentIs(value) && !CurrentIs('\0')) GoForwardOne();
            GoForward(value.Length);
        }

        bool InSkipEmpty;
        string? CommentString;
        private bool SkipEmpty(bool skipComments = true)
        {
            if (InSkipEmpty)
                return false;
            InSkipEmpty = true;
            bool skipped = false;
            while (spaceChars.Contains(Current) || (CurrentIs('/') && skipComments))
            {
                if (GoForwardIf('/'))
                {
                    if (GoForwardIf('/'))
                        SkipUntil('\n');
                    else if (GoForwardIf('*'))
                    {
                        bool docs;
                        if (docs = CurrentIs("*"))
                            CreateRestorePoint();
                        SkipUntil("*/");
                        if (docs)
                        {
                            int orgIndex = restorePoints.Pop();
                            int nextIndex = Index - 2;
                            string subString = TypeScript.Substring(orgIndex, nextIndex - orgIndex + 1);
                            CommentString = subString;
                        }
                    }
                    //else
                    //throw new Exception();
                }
                else
                    GoForwardOne();
                skipped = true;
            }
            InSkipEmpty = false;
            return skipped;
        }

        public bool CurrentIs(char value)
        {
            SkipEmpty();
            return Current == value;
        }

        public bool GoForwardIf(char value)
        {
            SkipEmpty();
            bool result = CurrentIs(value);
            if (result)
                GoForwardOne();
            return result;
        }

        public bool CurrentIs(string value)
        {
            SkipEmpty();
            for (int n = 0; n < value.Length; n++)
                if (CharAt(Index + n) != (value[n]))
                    return false;
            return true;
        }

        public bool GoForwardIf(string value)
        {
            SkipEmpty();
            for (int n = 0; n < value.Length; n++)
            {
                if (!CurrentIs(value[n]))
                {
                    GoForward(-n);
                    return false;
                }
                GoForwardOne();
            }
            return true;
        }

        public void GoForwardUntilEndBracket(params char[] inputArray)
        {
            List<char> asList = new List<char>(inputArray);
            char[] open = new char[inputArray.Length / 2];
            Array.Copy(inputArray, open, inputArray.Length / 2);
            asList.RemoveRange(0, inputArray.Length / 2);
            char[] closed = asList.ToArray();
            if (open.Contains('"'))
                InSkipEmpty = true;
            int parantheses = 0;
            do
            {
                if (open.Contains(Current) && (!open.All(v => closed.Contains(v)) || parantheses == 0))
                    parantheses++;
                else if (closed.Contains(Current))
                    parantheses--;
                GoForwardOne();
            } while (parantheses != 0);
            InSkipEmpty = false;
        }

        public string GetWord()
        {
            Console.WriteLine($"@{TypeScript.Substring(Index, TypeScript.Length-Index > 32 ? 32 : TypeScript.Length-Index)}");
            string result = "";
            SkipEmpty();
            while (wordChars.Contains(Current))
            {
                result += Current;
                GoForwardOne();
            }
            return result;
        }

        public string GetUntil(params char[] chars)
        {
            Console.WriteLine($"@{TypeScript.Substring(Index, TypeScript.Length - Index > 32 ? 32 : TypeScript.Length - Index)}");
            string result = "";
            SkipEmpty();
            while (!chars.Contains(Current))
            {
                result += Current;
                GoForwardOne();
            }
            return result;
        }

        public string GetQuotedWord()
        {
            char startChar = Current;
            if (startChar != '\'' && startChar != '"')
                throw new Exception();
            int startIndex = Index;
            GoForwardUntilEndBracket(startChar, startChar);
            return TypeScript.Substring(startIndex, Index - startIndex);
        }

        string GetName(string name)
        {
            if (Settings.RenameType?.ContainsKey(name)??false)
            {
                return Settings.RenameType[name];
            }
            //if (name == "void")
            //    name = "undefined";
            return name;
        }
        public CSharp.Lexicon.Type? DeduceType(string value)
        {
            if ((value.StartsWith("'") && value.EndsWith("'")) || (value.StartsWith("\"") && value.EndsWith("\"")))
            {
                return Context.GetOrCreateDataTypeByName(null, GetName("string"), (name) => new CSharp.Lexicon.Type(null, name));
            }
            int iv = 0;
            if (int.TryParse(value, out iv))
            {
                Context.GetOrCreateDataTypeByName(null, GetName("double"), (name) => new CSharp.Lexicon.Type(null, name));
            }
            if (value == "true" || value == "false")
            {
                return Context.GetOrCreateDataTypeByName(null, GetName("bool"), (name) => new CSharp.Lexicon.Type(null, name));
            }
            return null;// Context.GetOrCreateDataTypeByName(null, "object", () => new CSharp.Lexicon.Type(null, "object"));
        }

        public CSharp.Lexicon.Type? ParseSimpleType(ICSharpClosure closure)
        {
            SkipEmpty();
            string word = GetWord();
            if (word == "typeof")
                word = GetWord();
            else if (word == "this")
            {
                return closure as CSharp.Lexicon.Type;
            }
            CSharp.Lexicon.Type? typeResult = null;
            switch (word)
            {
                case "null":
                //typeResult = Context.GetOrCreateDataTypeByName(null, "");
                //break;
                case "any":
                    typeResult = Context.GetOrCreateDataTypeByName(null, GetName("object"), (name) => new CSharp.Lexicon.Type(Context.GetOrCreateNamespaceByName(null, "System") as ICSharpClosure, name));
                    break;
                case "number":
                    typeResult = Context.GetOrCreateDataTypeByName(null, GetName("double"), (name) => new CSharp.Lexicon.Type(Context.GetOrCreateNamespaceByName(null, "System") as ICSharpClosure, name));
                    break;
                case "boolean":
                case "false":
                case "true":
                    typeResult = Context.GetOrCreateDataTypeByName(null, GetName("bool"), (name) => new CSharp.Lexicon.Type(Context.GetOrCreateNamespaceByName(null, "System") as ICSharpClosure, name));
                    break;
                case "string":
                    typeResult = Context.GetOrCreateDataTypeByName(null, GetName("string"), (name) => new CSharp.Lexicon.Type(Context.GetOrCreateNamespaceByName(null, "System") as ICSharpClosure, name));
                    break;
                case "Array":
                //case "ArrayLike":
                    if (GoForwardIf('<'))
                    {
                        var type = ParseType(closure);
                        typeResult = new ArrayDataType(closure, type);
                        //GenericClass gClass = new GenericClass(Context.GetOrCreateNamespaceByName(null, "System") as ICSharpClosure, "Array");
                        //ParseGenericDeclaration(closure, gClass);
                        //typeResult = gClass;
                        GoForwardIf('>');
                    }
                    //if (CurrentIs('<'))
                    //{
                    //    GenericClass gClass = new GenericClass(Context.GetOrCreateNamespaceByName(null, "System") as ICSharpClosure, "Array");
                    //    ParseGenericDeclaration(closure, gClass);
                    //    typeResult = gClass;
                    //}
                    else
                        throw new Exception();
                    break;
                case "Function":
                    typeResult = Context.GetDataTypeByName("Function", (name) => new FunctionDataType(Context.GetOrCreateNamespaceByName(null, "System") as ICSharpClosure, name));
                    break;
                //case "typeof":
                //    var val = GetWord();
                //    typeResult = Context.GetDataTypeByName(val, (name)=> new LookupObjectType(null, name));
                //    break;
                case "":
                    if (CurrentIs('(') || CurrentIs('<'))
                    {
                        bool function = false;
                        if (CurrentIs('('))
                        {
                            CreateRestorePoint();
                            bool doubleBraced = CurrentIs("((");
                            if (doubleBraced)
                            {
                                GoForwardOne();
                            }
                            GoForwardUntilEndBracket('(', ')');
                            function = GoForwardIf("=>");
                            Restore();
                        }
                        if (!function)
                        {
                            GoForwardOne();
                            typeResult = ParseType(closure);
                            if (!GoForwardIf(')'))
                                throw new Exception();
                        }
                        else if (GoForwardIf('('))
                        {
                            bool doubleBraced = GoForwardIf('(');
                            var arg = ParseArguments(closure).Parameters;
                            if (!GoForwardIf("=>"))
                                throw new Exception();
                            var returnType = ParseType(closure);
                            typeResult = new MethodDelegate(Context.GetOrCreateNamespaceByName(null, "System") as ICSharpClosure, GetName(word), returnType, arg);
                            if (doubleBraced)
                            {
                                GoForwardIf(')');
                            }
                        }
                    }
                    else if (CurrentIs('{'))//anonymous interface
                    {
                        CreateRestorePoint();
                        if (GoForwardIf('{') && GoForwardIf('}'))
                        {
                            typeResult = Context.GetOrCreateDataTypeByName(null, GetName("object"), (name) => new CSharp.Lexicon.Type(Context.GetOrCreateNamespaceByName(null, "System") as ICSharpClosure, name));
                        }
                        else
                        {
                            Restore();
                            AnonymousInterface @interface = new AnonymousInterface(closure, closure.Name + "_Type_" + closure.AutoDefinedTypesCount);
                            closure.AutoDefinedTypesCount++;
                            Parse(@interface);
                            typeResult = @interface;
                        }
                    }
                    else if (CurrentIs('\'') || CurrentIs('"'))
                    {
                        string value = '"' + GetQuotedWord().Trim(new char[] { '\'', '"' }) + '"';
                        Literal literalClass = new Literal(closure, value, AccessSpecifier.Public, Context.GetOrCreateDataTypeByName(null, GetName("string")));
                        typeResult = literalClass;
                    }
                    break;
                default:
                    if (CurrentIs('<'))
                    {
                        GenericClass gClass = /*Context.GetOrCreateDataTypeByName<GenericClass>(closure, GetName(word+"<>"), (name) =>*/ new GenericClass(null, word)/*)*/;
                        ParseGenericDeclaration(closure, gClass);
                        typeResult = gClass;
                    }
                    else
                    {
                        typeResult = Context.GetOrCreateDataTypeByName(closure, GetName(word));
                    }
                    break;
            }
            if (typeResult != null)
            {
                typeResult.Name = GetName(typeResult.Name);
                if (GoForwardIf("[]"))
                {
                    typeResult = new ArrayDataType(null, typeResult);
                }
                if (GoForwardIf("="))
                {
                    var defaultType = ParseType(closure);
                    //string value = GetWord();
                    //if (value == "")
                    //{
                    //    value = GetUntil(',', '>', ')');
                    //}
                    typeResult.DefaultValue = defaultType;
                }
            }
            return typeResult;
        }

        public CSharp.Lexicon.Type ParseType(ICSharpClosure closure, string? name = null, bool isParsingUnion = false)
        {
            //GenericClass unionGenericType = Context.GetDataTypeByName("Union", (n) => new GenericUnionClass(Context.GetOrCreateNamespaceByName(null, "TCCS") as Namespace, "Union")) as GenericClass;
            //EnumeratedClass enumClass = name != null ? new EnumeratedClass(closure, name) : null;
            //GenericClass intersectionGenericType = Context.GetDataTypeByName("Intersection", (n) => new GenericUnionClass(Context.GetOrCreateNamespaceByName(null, "TCCS") as Namespace, "Intersection")) as GenericClass;
            SkipEmpty();
            //bool braced = GoForwardIf('(');
            CSharp.Lexicon.Type? typeResult;
            if (closure?.Name == "ChartLegendItem")
            {

            }
            if (CurrentIs('[')) //handles type like [number, number]
            {
                typeResult = new ArrayDataType(null, new CSharp.Lexicon.Type(Context.GetOrCreateNamespaceByName(null, "System") as ICSharpClosure, "object"));
                SkipUntil(']');
            }
            else
            {
                typeResult = ParseSimpleType(closure); 
            }
            //[]
            while (GoForwardIf('['))
            {
                if (!GoForwardIf(']'))
                    throw new Exception($"Format Exception @ {TypeScript.Substring(Index, 64)}");
                typeResult = Context.GetOrCreateArrayDataTypeByName(closure, typeResult);
            }
            if (!isParsingUnion)
            {
                List<CSharp.Lexicon.Type> pipedTypes = new List<CSharp.Lexicon.Type>();
                bool isAnd;
                if (typeResult is Literal)
                {
                    if (typeResult is Literal)
                        closure.Remove(typeResult);
                    pipedTypes.Add(typeResult);
                }
                while (GoForwardIf('|') || (isAnd = GoForwardIf('&')))
                {
                    if (pipedTypes.Count == 0)
                    {
                        if (typeResult is Literal)
                            closure.Remove(typeResult); 
                        pipedTypes.Add(typeResult);
                    }
                    //if (enumClass != null)
                    //{
                    //    if (enumClass.InnerObjects.Count == 0)
                    //    {
                    //        if (typeResult != null)
                    //        {
                    //            enumClass.Add(typeResult);
                    //        }
                    //        typeResult = enumClass;
                    //    }
                    //    if (typeResult != enumClass)
                    //    {
                    //        typeResult = enumClass;
                    //    }
                    //}
                    //else
                    //{
                    //    if (typeResult != enumClass && unionGenericType.GenericTypes.Count == 0)
                    //    {
                    //        if (typeResult != null)
                    //        {
                    //            unionGenericType.GenericTypes.Add(typeResult);
                    //        }
                    //        typeResult = unionGenericType;
                    //    }
                    //}
                    CSharp.Lexicon.Type type2 = ParseType(/*enumClass ?? */closure, null, true);
                    if (type2 is Literal)
                        closure.Remove(type2);
                    pipedTypes.Add(type2);
                    //if (enumClass != null && type2 is LiteralClass)
                    //{
                    //    enumClass.Add(type2);
                    //}
                    //else
                    //{
                    //    if (type2.Name != "void")
                    //    {
                    //        if (type2 is GenericClass gClass)
                    //        {
                    //            unionGenericType.GenericTypes.AddRange(gClass.GenericTypes);
                    //        }
                    //        else
                    //        {
                    //            unionGenericType.GenericTypes.Add(type2);
                    //        }
                    //    }
                    //}
                }
                if (pipedTypes.Count > 0)
                {
                    if (pipedTypes.Any(t=> t is Literal))
                    {
                        var enumC = new EnumeratedClass(closure, name??closure.Name+"_Type_"+closure.AutoDefinedTypesCount);
                        if (closure != null)
                        {
                            closure.AutoDefinedTypesCount++;
                        }
                        foreach(var t in pipedTypes)
                        {
                            enumC.Add(t);
                        }
                        typeResult = enumC;
                        //closure?.Add(typeResult);
                    }
                    else
                    {
                        if (isAnd)
                        {
                            GenericClass iType = Context.GetDataTypeByName("Intersection", (n) => new GenericUnionClass(Context.GetOrCreateNamespaceByName(null, "TCCS") as Namespace, "Intersection", internalType:true)) as GenericClass;
                            iType.GenericTypes = pipedTypes;
                            typeResult = iType;
                        }
                        else
                        {
                            GenericClass uType = Context.GetDataTypeByName("Union", (n) => new GenericUnionClass(Context.GetOrCreateNamespaceByName(null, "TCCS") as Namespace, "Union", internalType:true)) as GenericClass;
                            uType.GenericTypes = pipedTypes;
                            typeResult = uType;
                        }
                        closure?.Add(typeResult);
                    }
                }
                //cant have a union of one item
                //if (unionGenericType.GenericTypes.Count == 1)
                //{
                //    typeResult = unionGenericType.GenericTypes[0];
                //}
                //while (GoForwardIf('&'))
                //{
                //    if (intersectionGenericType.GenericTypes.Count == 0)
                //    {
                //        if (typeResult != null)
                //        {
                //            intersectionGenericType.GenericTypes.Add(typeResult);
                //        }
                //        typeResult = intersectionGenericType;
                //    }
                //    CSharp.Lexicon.Type type2 = ParseSimpleType(closure);
                //    if (type2.Name != "void")
                //    {
                //        if (type2 is GenericClass gClass)
                //        {
                //            intersectionGenericType.GenericTypes.AddRange(gClass.GenericTypes);
                //        }
                //        else
                //        {
                //            intersectionGenericType.GenericTypes.Add(type2);
                //        }
                //    }
                //}
                //cant have a union of one item
                //if (intersectionGenericType.GenericTypes.Count == 1)
                //{
                //    typeResult = intersectionGenericType.GenericTypes[0];
                //}
            }
            //if (braced)
            //    GoForwardIf(')');
            return typeResult;
        }

        public (List<MethodParameter> Parameters, List<MethodParameter> Properties) ParseArguments(ICSharpClosure closure)
        {
            if (closure?.Name == "Evented")
            {

            }
            List<MethodParameter> arguments = new List<MethodParameter>();
            List<MethodParameter> properties = new List<MethodParameter>();
            while (true)
            {
                bool optional = false;
                bool @public = GoForwardIf("public");
                bool @params = GoForwardIf("...");
                string name = GetWord();
                if (name == "")
                {
                    if (CurrentIs('{'))
                    {
                        name = "_p_";
                        GoForwardUntilEndBracket('{', '}');
                    }
                }
                if (GoForwardIf(')') || GoForwardIf(']'))
                    break;
                if (GoForwardIf('?'))
                    optional = true;
                if (!GoForwardIf(':') && !GoForwardIf("in"))///
                    throw new Exception();
                CSharp.Lexicon.Type paramType = ParseType(closure);
                MethodParameter parameter = new MethodParameter(null, paramType, name, isOptional:optional, isParams:@params);
                arguments.Add(parameter);
                if (@public)
                {
                    properties.Add(parameter);
                }
                GoForwardIf(',');
            }
            return (arguments, properties);
        }

        public Comment ParseComment()
        {
            if (CommentString == null)
                return new Comment();
            TypeScriptParser parser = new TypeScriptParser(CommentString, Context, Settings);
            Comment comment = new Comment();
            string? header = null;
            string? paramName = null;
            while (!parser.CurrentIs('\0'))
            {
                parser.GoForwardIf('*');
                if (!parser.GoForwardIf('@'))
                {
                    string g = string.Empty;
                    if (!parser.CurrentIs('\0'))
                    {
                        parser.CreateRestorePoint();
                        parser.InSkipEmpty = true;
                        parser.SkipUntil(CommentString.Substring(parser.Index).Contains('@') ? '@' : '\0');
                        parser.InSkipEmpty = false;
                        while (parser.CurrentIs('\0'))
                            parser.Index--;
                        int oldIndex = parser.restorePoints.Pop();
                        if (parser.Index > oldIndex)
                        {
                            g = CommentString.Substring(oldIndex, parser.Index - oldIndex - 1).Replace("*", "");
                        }
                    }
                    switch (header)
                    {
                        case "param":
                            if (!string.IsNullOrEmpty(paramName))
                                comment.Params.Add(new CommentParam(paramName, g));
                            break;
                        case "returns":
                        case "return":
                            comment.Return = new CommentReturn(g);
                            break;
                        case null:
                            //result.summary = g;
                            int paramStart = CommentString.IndexOf("@param");
                            if (paramStart < 0)
                                paramStart = CommentString.Length;
                            comment.Summary = new CommentSummary(CommentString.Substring(0, paramStart).Replace("*", ""));
                            break;
                    }
                }
                parser.GoForwardIf('*');
                header = parser.GetWord();
                paramName = null;
                if (parser.GoForwardIf('{'))
                    parser.SkipUntil('}');
                if (header == "param")
                {
                    parser.GoForwardIf('[');
                    paramName = parser.GetWord();
                    parser.GoForwardIf(']');
                }
            }
            CommentString = null;
            return comment;
        }

        public GenericType ParseGenericDeclaration(ICSharpClosure closure)
        {
            var name = GetWord();
            List<CSharp.Lexicon.Type> constraint = new List<CSharp.Lexicon.Type>();
            if (GoForwardIf('=')|| GoForwardIf("extends"))
            {
                CSharp.Lexicon.Type type = ParseType(closure);
                constraint.Add(type);
                //if (GoForwardIf('='))
                //{
                //    while (!CurrentIs(',')&& !CurrentIs('>'))
                //    {
                //        GoForwardOne();
                //    }
                //}
            }
            GenericType gType = new GenericType(closure, name, constraint);
            return gType;
        }

        public List<GenericType> ParseGenericDeclarations(ICSharpClosure closure)
        {
            List<GenericType> gds = new List<GenericType>();
            do
            {
                gds.Add(ParseGenericDeclaration(closure));
            } while (GoForwardIf(','));
            return gds;
        }

        public void ParseGenericDeclaration(ICSharpClosure closure, IGeneric @class)
        {
            if (GoForwardIf('<'))
            {
                List<CSharp.Lexicon.Type> gTypes = new List<CSharp.Lexicon.Type>();
                while (!GoForwardIf('>'))
                {
                    //string genericName = GetWord();
                    //if (!string.IsNullOrEmpty(genericName))
                    //{
                        CSharp.Lexicon.Type type = ParseType(closure);
                    //}
                    //else
                    //{
                    //    GenericType gType = new GenericType(@class, genericName);
                    //    @class.GenericTypes.Add(new GenericType(@class, genericName));
                    if (CurrentIs("extends") || CurrentIs("implements") || type.DefaultValue != null)
                    {
                        GenericType gType = new GenericType(@class, type.Name);
                        if (type.DefaultValue != null)
                        {
                            gType.Constraints.Add(type.DefaultValue);
                        }
                        type = gType;
                        while (GoForwardIf("extends") || GoForwardIf("implements"))
                        {
                            gType.Constraints.Add(ParseType(closure));
                        }
                    }
                    gTypes.Add(type);
                    //}
                    GoForwardIf(',');
                }
                @class.GenericTypes = gTypes;
            }
        }

        CSharp.Lexicon.Object ParseClass(ICSharpClosure closure, string classWord, bool @public, bool @abstract)
        {
            var interfaceName = GetWord();
            if (interfaceName == "Polyline")
            {

            }
            interfaceName = GetName(interfaceName);
            CSharp.Lexicon.Object? @object = null;
            Class? @class = null;
            if (CurrentIs('<'))
            {
                if (classWord == "interface")
                {
                    GenericInterface? @interface = closure.GetByName(interfaceName) as GenericInterface;
                    @interface = @interface ?? new GenericInterface(closure, interfaceName, @public ? AccessSpecifier.Public : AccessSpecifier.Private);
                    ParseGenericDeclaration(closure, @interface);
                    @object = @interface;
                    @class = @interface;
                }
                else
                {
                    if (@abstract)
                    {
                        GenericAbstractClass? aClass = closure.GetByName(interfaceName) as GenericAbstractClass;
                        ParseGenericDeclaration(closure, aClass);
                        aClass = aClass ?? new GenericAbstractClass(closure, interfaceName, @public ? AccessSpecifier.Public : AccessSpecifier.Private);
                        @object = aClass;
                        @class = aClass;
                    }
                    else
                    {
                        GenericClass? gClass = closure.GetByName(interfaceName) as GenericClass;
                        gClass = gClass ?? new GenericClass(closure, interfaceName, @public ? AccessSpecifier.Public : AccessSpecifier.Private);
                        ParseGenericDeclaration(closure, gClass);
                        @object = gClass;
                        @class = gClass;
                    }
                }
            }
            else
            {
                if (classWord == "interface")
                {
                    Interface? @interface = closure.GetByName(interfaceName) as Interface;
                    @interface = @interface ?? new Interface(closure, interfaceName);
                    @class = @interface;
                    @object = @interface;
                }
                //else if (classWord == "type")
                //{
                //    //@object = type;

                //    //Struct @struct = closure.GetByName(interfaceName) as Struct;
                //    //@struct = @struct ?? new Struct(closure, interfaceName, @public ? AccessSpecifier.Public : AccessSpecifier.Private);
                //    //@object = @struct;
                //    //GoForwardIf("=");
                //}
                else
                {
                    if (@abstract)
                    {
                        AbstractClass? aClass = closure.GetByName(interfaceName) as AbstractClass;
                        aClass = aClass ?? new AbstractClass(closure, interfaceName, @public ? AccessSpecifier.Public : AccessSpecifier.Private);
                        @object = aClass;
                        @class = aClass;
                    }
                    else
                    {
                        ConcreteClass? cClass = closure.GetByName(interfaceName) as ConcreteClass;
                        cClass = cClass ?? new ConcreteClass(closure, interfaceName, @public ? AccessSpecifier.Public : AccessSpecifier.Private);
                        @object = cClass;
                        @class = cClass;
                    }
                }
            }
            if (@public)
            {
                @object.Access = AccessSpecifier.Public;
            }
            bool implementing = false;
            bool extending = false;
            if (@class != null)
            {
                while ((extending = GoForwardIf("extends")) || (implementing = GoForwardIf("implements")))
                {
                    do
                    {
                        if (implementing)
                            @class.Implements.Add(ParseType(closure));
                        else if (extending)
                            @class.Extends.Add(ParseType(closure));
                    }
                    while (GoForwardIf(','));
                }
            }
            @object.Comment = ParseComment();
            Context.Add(@object);
            Parse(@object);
            return @class;
        }

        EnumType ParseEnum(ICSharpClosure closure, bool @public)
        {
            string enumName = GetWord();
            Comment comment = ParseComment();
            EnumType @enum = new EnumType(closure, enumName, @public ? AccessSpecifier.Public: AccessSpecifier.Private);
            @enum.Comment = ParseComment();

            if (!GoForwardIf('{'))
                throw new Exception();
            //int? ix = null;
            while (!GoForwardIf('}'))
            {
                var key = GetWord();
                if (string.IsNullOrWhiteSpace(key))
                    throw new Exception();
                object? value = null;
                if (GoForwardIf('='))
                {
                    value = GetWord();
                }
                EnumValue val = new EnumValue(value);
                val.Comment = ParseComment();
                @enum.Values.Add(key, val);
                GoForwardIf(',');
            }
            return @enum;
        }


        CSharp.Lexicon.Type ParseAlias(ICSharpClosure closure)
        {
            var name = GetWord();
            //List<MethodParameter> parameters;
            //if (GoForwardIf('<'))
            //{
            //    parameters = ParseArguments(closure).Parameters;
            //}
            //Delegate dlg = new CSharp.Lexicon.Delegate(closure, name, AccessSpecifier.Public, )
            //CSharp.Lexicon.Type type = closure.GetByName(name) as CSharp.Lexicon.Type;
            List<GenericType> gType = new List<GenericType>();
            if (GoForwardIf('<'))
            {
                gType = ParseGenericDeclarations(closure);
                GoForwardIf('>');
            }
            GoForwardIf("=");
            if (name == "LineCapShape")
            {

            }
            if (name == "Zoom")
            {

            }
            if (name == "StyleFunction")
            {

            }
            if (name == "All")
            {

            }
            var alias = ParseType(closure, name);
            GoForwardIf(';');
            if (alias is EnumeratedClass && alias.Name == name)
            {
                //alias.Name = name;
                return alias;
            }
            var t = new AliasType(closure, name, alias, gType);
            //type = type ?? new CSharp.Lexicon.Type(closure, name, alias);
            return t;
        }

        Namespace? globalNamespace;
        Namespace? currentNamespace;
        Class? currentClass;
        ICSharpClosure? currentClosure;

        public Method ParseMethod(ICSharpClosure closure, AccessSpecifier access, bool @static)
        {
            var methodName = GetWord();
            List<GenericType> gType = new List<GenericType>();
            if (GoForwardIf('<'))
            {
                gType = ParseGenericDeclarations(closure);
                GoForwardIf('>');
            }
            GoForwardIf('(');
            var method = new Method(closure, methodName, access, null, @static);
            method.GenericArgs = gType;
            method.Parameters = ParseArguments(method).Parameters;
            if (GoForwardIf(':'))
            {
                method.Return = ParseType(method);
            }
            GoForwardIf(';');
            return method;// new Method(closure, methodName, access, retn, @static, args);
        }

        public void Parse(ICSharpClosure closure)
        {
            if (!GoForwardIf('{'))
            {
                //    throw new Exception();
            }
            bool @readonly = false;
            bool @static = false;
            bool @protected = false;
            bool @public = Settings.PublicAll;
            bool @abstract = false;
            bool indexer = false;
            List<GenericType>? genericArgs = null;
            while (true)
            {
                bool quoted = CurrentIs('"') || CurrentIs('\'');
                string word;
                if (!quoted)
                    word = GetWord();
                else
                {
                    int oldIdx = Index;
                    GoForwardUntilEndBracket('"', '\'', '"', '\'');
                    word = /*'\x1' + */TypeScript.Substring(oldIdx + 1, Index - 2 - oldIdx);
                }
                switch (word)
                {
                    case "":
                        if (GoForwardIf('}'))
                        {
                            if (closure.ClosureType == ClosureType.Struct)
                            {
                                if (!GoForwardIf(';'))
                                {
                                    throw new Exception();
                                }
                            }
                            return;
                        }
                        else if (CurrentIs('(') || CurrentIs('<'))
                            goto default;
                        else if (GoForwardIf('['))
                        {
                            indexer = true;
                            goto default;
                        }
                        else
                        {
                            if (Index == TypeScript.Length)
                            {
                                return;
                            }
                            throw new Exception($"Unhandled Typescript syntax @ {TypeScript.Substring(Index, 64)}");
                        }
                    case "static":
                    case "let":
                    case "var":
                        if (CurrentIs(':') || CurrentIs('(') || CurrentIs('?'))
                            goto default;
                        @static = true;
                        break;
                    case "export":
                        @public = true;
                        break;
                    case "protected":
                        if (CurrentIs(':') || CurrentIs('(') || CurrentIs('?'))
                            goto default;
                        @protected = true;
                        break;
                    case "abstract":
                        if (CurrentIs(':') || CurrentIs('(') || CurrentIs('?'))
                            goto default;
                        @abstract = true;
                        break;
                    case "declare":
                        if (CurrentIs(':') || CurrentIs('(') || CurrentIs('?'))
                            goto default;
                        break;
                    case "readonly":
                        if (CurrentIs(':') || CurrentIs('(') || CurrentIs('?'))
                            goto default;
                        @readonly = true;
                        break;
                    case "const":
                        if (CurrentIs(':') || CurrentIs('('))
                            goto default;
                        @static = true;
                        goto case "readonly";
                    case "function":
                        ParseMethod(closure, AccessSpecifier.Public, true);
                        @static = false;
                        @public = false;
                        break;
                    case "namespace":
                    case "module":
                        CreateRestorePoint();
                        string name = GetWord();
                        if (name == "" &&CurrentIs("'"))
                        {
                            name = GetQuotedWord();
                            name = name.Trim(new char[] { '\'', '"' });
                        }
                        if (CurrentIs('{'))
                        {
                            //if (closure.ClosureType == ClosureType.Namespace && closure.Name != "Global")
                            //{
                            //    var @namespace = (closure as Namespace).GetNamespaceByName(GetName(name));
                            //    @namespace = @namespace ?? new Namespace(GetName(name), (closure as Namespace));
                            //    Parse(@namespace);
                            //}
                            //else
                            //{
                            //    var @namespace = Context.GetOrCreateNamespaceByQualifiedName(GetName(name));
                            //    Parse(@namespace);
                            //}
                            var @class = Context.GetOrCreateDataTypeByName(closure, name, (nm) => new ConcreteClass(closure, name, AccessSpecifier.Public, true));
                            Parse(@class);
                            @readonly = false;
                            @static = false;
                            @protected = false;
                            @public = Settings.PublicAll;
                            @abstract = false;
                            indexer = false;
                        }
                        else
                        {
                            Restore();
                            goto default;
                        }
                        break;
                    case "type":
                        if (CurrentIs('?') || CurrentIs(':'))
                        {
                            goto default;
                        }
                        ParseAlias(closure);
                        @readonly = false;
                        @static = false;
                        @protected = false;
                        @public = Settings.PublicAll;
                        @abstract = false;
                        indexer = false;
                        break;
                    case "interface":
                    case "class":
                        if (!CurrentIs(':'))
                        {
                            ParseClass(closure, word, @public, @abstract);
                            @readonly = false;
                            @static = false;
                            @protected = false;
                            @public = Settings.PublicAll;
                            @abstract = false;
                            indexer = false;
                        }
                        else
                        {
                            goto default;
                        }
                        break;
                    case "enum":
                        ParseEnum(closure, @public);
                        @readonly = false;
                        @static = false;
                        @protected = false;
                        @public = Settings.PublicAll;
                        @abstract = false;
                        indexer = false;
                        break;
                    default:
                        if (word == "manhattanLength")
                        {

                        }
                        Comment comment = ParseComment();
                        bool isOptional = GoForwardIf('?');
                        if (GoForwardIf('<'))
                        {
                            genericArgs = ParseGenericDeclarations(closure);
                            GoForwardIf('>');
                        }
                        if (GoForwardIf('(') || indexer)
                        {
                            var arguments = ParseArguments(closure);
                            CSharp.Lexicon.Type returnType;
                            var optional = GoForwardIf("?");
                            if (GoForwardIf(':'))
                            {
                                returnType = ParseType(closure);
                            }
                            else
                            {
                                returnType = new CSharp.Lexicon.Type(null, "void");
                            }
                            if (word == "constructor")
                            {
                                word = closure.Name;
                                word = word.Split(new char[] { '<' })[0];
                                CSharp.Lexicon.Object? @object = closure as CSharp.Lexicon.Object;
                                if (@object != null)
                                {
                                    returnType = new CSharp.Lexicon.Type(null, "");
                                    var properties = arguments.Properties.ConvertAll(p => new Property(@object, p.Name, AccessSpecifier.Public, p.Type));
                                    @object.Properties.AddRange(properties);
                                }
                                else
                                {
                                    throw new Exception();
                                }
                            }
                            Method method = new Method(closure, word, @protected ? AccessSpecifier.Protected : AccessSpecifier.Public, returnType, @static, arguments.Parameters);
                            method.GenericArgs = genericArgs;
                            genericArgs = null;
                            //skip method definition
                            if (CurrentIs("{"))
                            {
                                GoForwardUntilEndBracket('{', '}');
                            }
                            method.Comment = comment;
                            method.Abstract = @abstract;
                            method.IsIndexer = indexer;
                            @readonly = false;
                            @static = false;
                            @protected = false;
                            @public = Settings.PublicAll;
                            @abstract = false;
                            indexer = false;
                        }
                        else if (GoForwardIf(':'))
                        {
                            if (@static && @readonly && false)
                            {
                                CreateRestorePoint();
                                var value = GetWord();
                                if (value == "")
                                {
                                    value = GetQuotedWord();
                                }
                                CSharp.Lexicon.Type type = DeduceType(value);
                                if (type != null)
                                {
                                    if (type.Name == "string")
                                    {
                                        value = "\"" + value.Trim("'".ToCharArray()) + "\"";
                                    }
                                    Variable variable = new Variable(closure, type, word, @protected ? AccessSpecifier.Protected : AccessSpecifier.Public, @static, @readonly, value);
                                    variable.Comment = comment;
                                }
                                else
                                {
                                    Restore();
                                    CSharp.Lexicon.Type propertyType = ParseType(closure);
                                    Property property = new Property(closure, word, @protected ? AccessSpecifier.Protected : AccessSpecifier.Public, propertyType, @static, @readonly, isOptional);
                                    property.Comment = comment;
                                    property.Abstract = @abstract;
                                }
                            }
                            else
                            {
                                CSharp.Lexicon.Type propertyType = ParseType(closure);
                                Property property = new Property(closure, word, @protected ? AccessSpecifier.Protected : AccessSpecifier.Public, propertyType, @static, @readonly, isOptional);
                                property.Comment = comment;
                                property.Abstract = @abstract;
                            }
                            @readonly = false;
                            @static = false;
                            @protected = false;
                            @public = Settings.PublicAll;
                            @abstract = false;
                            indexer = false;
                        }
                        else if (GoForwardIf('='))
                        {
                            bool singleQuoted = CurrentIs('\'');
                            bool doubleQuoted = CurrentIs('"');
                            string value = "";
                            if (singleQuoted || doubleQuoted)
                            {
                                value = GetQuotedWord();
                            }
                            else
                            {
                                value = GetWord();
                            }
                            Variable variable = new Variable(closure, Context.GetOrCreateDataTypeByName(closure, "object"), word, @public ? AccessSpecifier.Public : AccessSpecifier.Private, @static, @readonly, value);
                        }
                        if (closure.ClosureType == ClosureType.Struct)
                        {
                            if (!CurrentIs(',') & !CurrentIs('}'))
                            {
                                //throw new Exception();
                            }
                            if (!CurrentIs('}'))
                                GoForwardIf(',');
                        }
                        else if (closure.ClosureType == ClosureType.Class || closure.ClosureType == ClosureType.Interface || closure.ClosureType == ClosureType.Namespace || closure.ClosureType == ClosureType.AnonymousInterface)
                        {
                            if (!CurrentIs(';') & !CurrentIs('}'))
                            {
                                //throw new Exception();
                            }
                            if (GoForwardIf('<'))
                            {
                                genericArgs = ParseGenericDeclarations(closure);
                                GoForwardIf('>');
                            }
                            if (!CurrentIs('}'))
                            {
                                GoForwardIf(';');
                                if (closure.ClosureType == ClosureType.AnonymousInterface)
                                {
                                    GoForwardIf(',');
                                }
                            }
                        }
                        break;
                }
            }
        }

        public ICSharpClosure Parse(string globalName)
        {
            currentClosure = globalNamespace = new Namespace(globalName);
            Context.Add(globalNamespace);
            Context.Add(new Namespace("System", alreadyDefined:true));
            Context.Add(new Namespace("TCCS", alreadyDefined: true));
            Parse(globalNamespace);
            return currentClosure;
            //bool @public = false;
            //bool @abstract = false;
            //while (true)
            //{
            //Back:
            //    if (CurrentIs('\0'))
            //        return;
            //    string word = GetWord();
            //    switch (word)
            //    {
            //        case "namespace":
            //        case "module":
            //            string name = GetWord();
            //            if (!GoForwardIf('{'))
            //                throw new Exception();
            //            var @namespace = Context.GetOrCreateNamespaceByQualifiedName(name);
            //            //new Namespace(name, currentNamespace);
            //            //if (currentNamespace == null)
            //            //{
            //            //    Context.Add(@namespace);
            //            //}
            //            currentClosure = currentNamespace = @namespace;
            //            break;
            //        case "declare":
            //            goto Back;
            //        case "export":
            //            @public = true;
            //            goto Back;
            //        case "abstract":
            //            @abstract = true;
            //            goto Back;
            //        //case "const":
            //        //case "function":
            //        //case "var":
            //        //case "let":
            //        //    //GoForward(-word.Length);
            //        //    //currentClass = currentNamespace.GlobalClass;
            //        //    //if (ParseClassLine(currentClosure, currentClass))
            //        //    //    throw new Exception();
            //        //    break;
            //        case "type":
            //        case "interface":
            //        case "class":
            //            ParseClass(currentNamespace ?? globalNamespace, word, @public, @abstract);
            //            @public = false;
            //            @abstract = false;
            //            break;
            //        case "enum":
            //            ParseEnum(currentNamespace ?? globalNamespace);
            //            break;
            //        default:
            //            if (GoForwardIf('}'))
            //            {
            //                currentNamespace = currentNamespace.Parent;
            //            }
            //            else
            //            {
            //                throw new Exception("Unhandled Typesript syntax");
            //            }
            //            break;
            //    }
            //    CommentString = null;
            //}
        }
    }
}
