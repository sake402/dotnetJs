using dotnetJs.Translator.CSharpToJavascript;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnetJs.Translator.CSharpToJavascript
{
    public partial class TranslatorSyntaxVisitor
    {
        public HashSet<INamedTypeSymbol> Dependencies { get; private set; } = new();
        Stack<BaseTypeDeclarationSyntax> _currentTypes = new Stack<BaseTypeDeclarationSyntax>();

        BaseTypeDeclarationSyntax CurentType => _currentTypes.Peek();

        public override void VisitPredefinedType(PredefinedTypeSyntax node)
        {
            EnsureImported(node);
            var symbol = _global.GetTypeSymbol(node, this/*, out _, out _*/).GetTypeSymbol();
            var meta = _global.GetRequiredMetadata(symbol);
            var name = meta.OverloadName ?? Utilities.ResolvePredefinedTypeName(node);
            Writer.Write(node, name);
            base.VisitPredefinedType(node);
        }

        public override void VisitNullableType(NullableTypeSyntax node)
        {
            var nullable = _global.GetTypeSymbol("System.Nullable<>", this/*, out _, out _*/);
            var metadata = _global.GetRequiredMetadata(nullable);
            EnsureImported(node.ElementType);
            //Writer.Write(node, $"{metadata.InvocationName}(");
            Visit(node.ElementType);
            //Writer.Write(node, ")");
            //base.VisitNullableType(node);
        }

        public override void VisitFunctionPointerType(FunctionPointerTypeSyntax node)
        {
            Writer.Write(node, $"{_global.GlobalName}.System.Object");
            //base.VisitFunctionPointerType(node);
        }

        public void WriteTypeDeclaration(BaseTypeDeclarationSyntax node, IEnumerable<ParameterSyntax>? primaryConstructorParameters, IEnumerable<MemberDeclarationSyntax>? members, Action? writePrologue = null, Action? writeEpilogue = null)
        {
            var previousClosure = CurrentClosure;

            var typeSymbol = (INamedTypeSymbol)OpenClosure(node);
            if (_global.ShouldExportType(typeSymbol, this))
            {
                var typeMetadata = _global.GetRequiredMetadata(typeSymbol);// SyntaxSymbols.GetValueOrDefault(node)?.First() ?? default;
                                                                           //we already processed a partial member
                lock (_global)
                {
                    if (_global.ProcessedTypeNodes.Contains(typeMetadata.FullName))
                    {
                        CloseClosure();
                        return;
                    }
                    if (node.Parent is not BaseTypeDeclarationSyntax)
                    {
                        Writer = new ScriptWriter();
                        TypeWriters[typeSymbol] = Writer;
                        _global.TypeVisitors[typeSymbol] = this;
                        _global.TypeWriters[typeSymbol] = Writer;
                    }
                    else
                    {
                        var mclassName = node.Identifier.ValueText;
                        if (typeSymbol.Arity > 0)
                        {
                            mclassName += "<" + string.Join(",", Enumerable.Range(1, typeSymbol.Arity)) + ">";
                        }
                        previousClosure.DefineIdentifierType(/*classMetadata.LocalName ?? */mclassName, CodeSymbol.From(typeSymbol));
                    }

                    //add all member symbol to this scope
                    members ??= node.ChildNodes().OfType<MemberDeclarationSyntax>();
                    if (node.Modifiers.Any(m => m.ValueText == "partial"))
                    {
                        var type = node.GetType();
                        //var partialTypes = global.TypeNodes.GetValueOrDefault(fullTypeName);
                        if (typeMetadata.DeclaringReferences.Count() > 1)
                        {
                            foreach (var partial in typeMetadata.DeclaringReferences)
                            {
                                var sm = _global.Compilation.GetSemanticModel(partial.SyntaxTree);
                                if (!_semanticModels.Contains(sm))
                                    _semanticModels.Add(sm);
                            }
                            var usingNamespaces = typeMetadata.DeclaringReferences.SelectMany(c => c.SyntaxTree.GetRoot().ChildNodes().OfType<UsingDirectiveSyntax>());
                            foreach (var us in usingNamespaces)
                            {
                                Visit(us);
                            }
                            members = typeMetadata.DeclaringReferences.Select(e => e.GetSyntax()).SelectMany(c => c.ChildNodes().OfType<MemberDeclarationSyntax>()).ToList();
                            _global.ProcessedTypeNodes.Add(typeMetadata.FullName);
                        }
                    }
                }
                bool _static = node.Modifiers.Any(m => m.ValueText == "static") || node.IsKind(SyntaxKind.EnumDeclaration)/* is EnumDeclarationSyntax*/;
                bool isBootClass = _global.HasAttribute(typeSymbol, typeof(BootAttribute).FullName, this, false, out _);
                string? _base = null;
                string? mixImplementedInterfaces = null;
                string[]? implementedInterfaces = null;
                var interfaces = typeSymbol.Interfaces;
                const bool useInterfaceMixin = false;
                if (!useInterfaceMixin)
                {
                    //string InterfaceOutputName(ITypeSymbol _interface)
                    //{
                    //    string? Ts = null;
                    //    if (_interface is INamedTypeSymbol nt && nt.Arity > 0)
                    //    {
                    //        Ts = string.Join(", ", nt.TypeArguments.Select(t =>
                    //        {
                    //            return InterfaceOutputName(t);
                    //        }));
                    //    }
                    //    if (Ts != null)
                    //    {
                    //        Ts = "(" + Ts + ")";
                    //    }
                    //    return $"{_interface.ComputeOutputTypeName(_global)}{Ts}";
                    //}
                    implementedInterfaces = interfaces.Where(i => _global.ShouldExportType(i, this)).Select(e => e.ComputeOutputTypeName(_global)).ToArray();
                }
                else
                {
                    foreach (var _interface in interfaces.Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default))
                    {
                        Dependencies.Add(_interface);
                        if (!_global.ShouldExportType(_interface!, this))
                            continue;
                        if (mixImplementedInterfaces == null)
                            mixImplementedInterfaces = $"{{0}}";
                        EnsureImported((ITypeSymbol)_interface);
                        var interfaceName = _global.GetRequiredMetadata(_interface).OverloadName;
                        string? Ts = null;
                        if (_interface.Arity > 0)
                        {
                            Ts = string.Join(", ", _interface.TypeArguments.Select(t =>
                            {
                                return t.ComputeOutputTypeName(_global);
                            }));
                        }
                        mixImplementedInterfaces = $"{interfaceName}({Ts}{(Ts != null ? ", " : "")}{mixImplementedInterfaces})";
                    }
                }
                if (typeSymbol.Arity > 0)
                {
                    foreach (var a in typeSymbol.TypeParameters)
                    {
                        CurrentClosure.DefineIdentifierType(a.Name, CodeSymbol.From(a));
                    }
                }
                string? classDefinition = null;
                string? closingClassDeclaration = null;
                string? closingClassDeclaration2 = null;
                var fullClassName = /*typeMetadata.OverloadName ??*/ typeMetadata.FullName.RemoveGenericParameterNames(out _) ?? _global.ResolveTypeName(node);
                if (_global.OutputMode.HasFlag(OutputMode.Global) && fullClassName.StartsWith(_global.GlobalName + "."))
                {
                    fullClassName = fullClassName.Substring(_global.GlobalName.Length + 1);
                }
                var classNameSegments = fullClassName.Split('.');
                var className = classNameSegments[classNameSegments.Length - 1];
                var classNamespace = string.Join(".", classNameSegments.Take(classNameSegments.Length - 1));
                bool isInnerClass = false;
                //bool hasMixin = false;
                if (!_static)
                {
                    var systemObject = _global.GetTypeSymbol("System.Object", this/*, out _, out _*/);
                    if (typeSymbol.BaseType != null)
                    {
                        if (isBootClass && typeSymbol.BaseType.Equals(systemObject, SymbolEqualityComparer.Default))
                        {

                        }
                        else
                        {
                            if (typeSymbol.Name == "SerializableAttribute")
                            {

                            }
                            Dependencies.Add(typeSymbol.BaseType);
                            var baseMeta = _global.GetRequiredMetadata(typeSymbol.BaseType);
                            var baseName = typeSymbol.BaseType.ComputeOutputTypeName(_global);
                            //if (typeSymbol.BaseType.Arity > 0)
                            //{
                            //    string Ts = string.Join(", ", typeSymbol.BaseType.TypeArguments.Select(t =>
                            //    {
                            //        return t.ComputeOutputTypeName(_global);
                            //    }));
                            //    baseName += "(" + Ts + ")";
                            //}
                            EnsureImported(typeSymbol.BaseType);
                            if (mixImplementedInterfaces != null)
                            {
                                _base = $" extends {string.Format(mixImplementedInterfaces, baseName)}";
                            }
                            else if (implementedInterfaces?.Length > 0)
                            {
                                _base = $" extends $.$mix({baseName}, {string.Join(", ", implementedInterfaces)})";
                            }
                            else if (typeSymbol.BaseType.Arity > 0)
                            {
                                //string genericTypes = string.Join(", ", symbol.BaseType.TypeArguments.Select(t => t.ComputeOutputTypeName(global)));
                                //_base = $" extends {baseName}({genericTypes})";
                                _base = $" extends {baseName}";
                            }
                            else
                            {
                                _base = $" extends {baseName}";
                            }
                        }
                    }
                    else if (!isBootClass && node.IsKind(SyntaxKind.ClassDeclaration) || node.IsKind(SyntaxKind.InterfaceDeclaration))
                    {
                        if (mixImplementedInterfaces != null)
                        {
                            _base = $" extends {string.Format(mixImplementedInterfaces, node.IsKind(SyntaxKind.InterfaceDeclaration) ? "Mixin" : _global.OutputMode.HasFlag(OutputMode.Global) ? _global.GlobalName + ".System_Object" : "Object")}";
                        }
                        else if (implementedInterfaces?.Length > 1)
                        {
                            _base = $" extends {_global.GlobalName}.$mix({string.Join(", ", implementedInterfaces)})";
                        }
                        else if (implementedInterfaces?.Length > 0)
                        {
                            _base = $" extends {implementedInterfaces.Single()}";
                        }
                        else if (className != "dotnetJs.System.Object" && className != "Object")
                        {
                            var systemObjectMetadata = _global.GetRequiredMetadata(systemObject);
                            if (!node.IsKind(SyntaxKind.InterfaceDeclaration))
                            {
                                if (!typeSymbol.Equals(systemObject, SymbolEqualityComparer.Default))
                                    _base = $" extends " + systemObjectMetadata.InvocationName;
                            }
                            else if (useInterfaceMixin)
                            {
                                _base = $" extends (Mixin??{_global.GlobalName}.$nomix)";
                            }
                        }
                    }
                }
                string genericArgs = string.Join(", ", (node as TypeDeclarationSyntax)?.TypeParameterList?.Parameters.Select(t => $"{(t.VarianceKeyword.ValueText?.Length > 0 ? $"/*{t.VarianceKeyword.ValueText}*/ " : "")}{t.Identifier.ValueText}") ?? Enumerable.Empty<string>());
                bool isInterface = /*useInterfaceMixin &&*/ node.IsKind(SyntaxKind.InterfaceDeclaration);
                bool usingInterfaceMixin = isInterface && useInterfaceMixin;
                bool hasGeneric = genericArgs?.Length > 0;
                bool hasMixinOrGeneric = isInterface || hasGeneric;
                if (isBootClass)
                {
                    //for (int i = 1; i < classNameSegments.Length; i++)
                    //{
                    //    var path = string.Join(".", classNameSegments.Take(i));
                    //    Writer.WriteLine(node, $"$.{path} ??= {{ }};", true);
                    //}
                    classDefinition = $"$.$ns(\"{fullClassName}\", {(typeSymbol.Arity > 0 ? $"({genericArgs}{(hasGeneric && isInterface ? ", " : "")}{(isInterface ? "Mixin" : "")}) => " : "")}class {"" ?? className}{_base}";
                    closingClassDeclaration = ");";
                }
                else
                {
                    classDefinition = $"$asm.$cls(\"{fullClassName}\", ({genericArgs}) => {(hasGeneric ? $"$asm.$gt(\"{fullClassName}\", [{genericArgs}], () => " : "")}class {"" ?? className}{_base}";
                    //classDefinition = $"$asm.$cls(\"{fullClassName}\", ({genericArgs}{(hasGeneric && usingInterfaceMixin ? ", " : "")}{(usingInterfaceMixin ? "Mixin" : "")}) => {(hasGeneric || isInterface ? $"$asm.${(isInterface && hasGeneric ? "gm" : isInterface && !hasGeneric ? "mx" : "gt")}(\"{fullClassName}\", [{genericArgs}{(hasGeneric && usingInterfaceMixin ? ", " : "")}{(usingInterfaceMixin ? "Mixin" : "")}], () => " : "")}class {"" ?? className}{_base}";
                    //if (genericArgs?.Length > 0 || isInterface)
                    if (hasGeneric)
                    {
                        closingClassDeclaration = "));";
                    }
                    else
                    {
                        closingClassDeclaration = ");";
                    }
                }
                string openingClassDefinition = "{";
                if (node.Parent is TypeDeclarationSyntax)
                {
                    var localClassName = className.Replace("<", "$").Replace(",", "$").Replace(">", "$");
                    var parent = _global.GetTypeSymbol(node.Parent, this/*, out _, out _*/);
                    var parentMetadata = _global.GetRequiredMetadata(parent);
                    isInnerClass = true;
                    Writer.WriteLine(node, $"static $_{localClassName};", true);
                    Writer.WriteLine(node, $"static {(!hasGeneric ? "get " : "")}{localClassName}({genericArgs})", true);
                    Writer.WriteLine(node, $"{{", true);
                    classDefinition = $"return {parentMetadata.InvocationName}.$_{localClassName} ??= {classDefinition}";
                    if (hasGeneric)
                    {
                        closingClassDeclaration = $"))({genericArgs});";
                    }
                    //closingClassDeclaration = ");";
                    closingClassDeclaration2 = "}";
                }

                //if (node.Parent is TypeDeclarationSyntax)
                //{
                //    //inner class definition
                //    isInnerClass = true;
                //    Writer.WriteLine(node, $"static $_{className};", true);
                //    if ((node is TypeDeclarationSyntax t && t.Arity > 0))
                //    {
                //        genericArgs = string.Join(", ", (node as TypeDeclarationSyntax)?.TypeParameterList?.Parameters.Select(t => $"{(t.VarianceKeyword.ValueText?.Length > 0 ? $"/*{t.VarianceKeyword.ValueText}*/ " : "")}{t.Identifier.ValueText}") ?? Enumerable.Empty<string>());
                //        Writer.WriteLine(node, $"static get {className}()", true);
                //        Writer.WriteLine(node, $"{{", true);
                //        classDefinition = $"return $_{className} ??= ({genericArgs}) => {_global.GlobalName}.Assembly.GetGeneric($asm, [{genericArgs}], class {className}{_base}";
                //        closingClassDeclaration = ");";
                //        closingClassDeclaration2 = "}";
                //    }
                //    else
                //    {
                //        Writer.WriteLine(node, $"static get {className}()", true);
                //        Writer.WriteLine(node, $"{{", true);
                //        classDefinition = $"return $_{className} ??= {_global.GlobalName}.Assembly.Define($asm, class {className}{_base}";
                //        closingClassDeclaration = ");";
                //        closingClassDeclaration2 = "}";
                //    }
                //}
                //else if (node.IsKind(SyntaxKind.InterfaceDeclaration)/* is InterfaceDeclarationSyntax*/ || (node is TypeDeclarationSyntax t && t.Arity > 0))
                //{
                //    genericArgs = string.Join(", ", (node as TypeDeclarationSyntax)?.TypeParameterList?.Parameters.Select(t => $"{(t.VarianceKeyword.ValueText?.Length > 0 ? $"/*{t.VarianceKeyword.ValueText}*/ " : "")}{t.Identifier.ValueText}") ?? Enumerable.Empty<string>());
                //    if (node.IsKind(SyntaxKind.InterfaceDeclaration)/* is InterfaceDeclarationSyntax*/)
                //    {
                //        classDefinition = $"{(node.Parent is TypeDeclarationSyntax ? "static" : _global.OutputMode.HasFlag(OutputMode.Global) ? "" : "export const ")}$asm.$CLS(\"{fullClassName}\", ({genericArgs}{(genericArgs?.Length > 0 ? ", " : "")}Mixin) => {_global.GlobalName}.Assembly.Mixin($asm, [{"" ?? genericArgs}{(false && genericArgs?.Length > 0 ? ", " : "")}Mixin], class {className} extends Mixin";
                //        closingClassDeclaration = "));";
                //        hasMixin = true;
                //    }
                //    else
                //    {
                //        classDefinition = $"{(node.Parent is TypeDeclarationSyntax ? "static" : _global.OutputMode.HasFlag(OutputMode.Global) ? "" : "export const ")}$asm.$CLS(\"{fullClassName}\", ({genericArgs}) => {_global.GlobalName}.Assembly.GetGeneric($asm, [{genericArgs}], class {className}{_base}";
                //        closingClassDeclaration = "));";
                //    }
                //}
                //else
                //{
                //    classDefinition = $"{(_global.OutputMode.HasFlag(OutputMode.Global) ? "" : "export const ")}$asm.$CLS(\"{fullClassName}\", class {className}{_base}";
                //    closingClassDeclaration = ")";
                //}
                //if (!isInnerClass)
                //{
                //    Writer.WriteLine(node, $"$asm.$NS(\"{string.Join(".", classNameSegments.Take(classNameSegments.Length - 1))}\", function($ns)", true);
                //    Writer.WriteLine(node, $"{{", true);
                //}
                Writer.WriteLine(node, classDefinition, true);
                Writer.WriteLine(node, openingClassDefinition, true);

                if (primaryConstructorParameters?.Any() ?? false)
                {
                    WritePrimaryConstructor(node, typeSymbol, primaryConstructorParameters);
                }

                //define every member of this class in the class scope so they are available for type checking
                var membersSymbol = typeSymbol.GetMembers(null, _global)
                //    .Where(m =>
                //{
                //    if (m is IMethodSymbol mm && mm.ExplicitInterfaceImplementations.Any())
                //    {
                //        return false;
                //    }
                //    else if (m is IPropertySymbol pt && pt.ExplicitInterfaceImplementations.Any())
                //    {
                //        return false;
                //    }
                //    return true;
                //})
                    .GroupBy(m =>
                {
                    //var metadata = _global.GetRequiredMetadata(m);
                    //return metadata.OriginalOverloadName;
                    var name = m.Name;
                    //if (!name.Contains(".") && m is IMethodSymbol mm && mm.ExplicitInterfaceImplementations.Any())
                    //{
                    //    var e = mm.ExplicitInterfaceImplementations.First();
                    //}
                    //else if (!name.Contains(".") && m is IPropertySymbol pt && pt.ExplicitInterfaceImplementations.Any())
                    //{
                    //    var e = pt.ExplicitInterfaceImplementations.First();
                    //}
                    if (m is IMethodSymbol mt && mt.Arity > 0)
                    {
                        name = name + "<" + string.Join(",", Enumerable.Range(1, mt.Arity).Select(c => "")) + ">";
                    }
                    if (m is INamedTypeSymbol tt && tt.Arity > 0)
                    {
                        name = name + "<" + string.Join(",", Enumerable.Range(1, tt.Arity).Select(c => "")) + ">";
                    }
                    return name;
                }).ToList();

                writePrologue?.Invoke();

                foreach (var m in membersSymbol)
                {
                    if (m.Count() == 1)
                        CurrentClosure.DefineIdentifierType(m.Key, CodeSymbol.From(m.Single()));
                    else
                    {
                        CurrentClosure.DefineIdentifierType(m.Key, CodeSymbol.From(new MemberSymbolOverload()
                        {
                            Overloads = m.ToList()!
                        }));
                    }
                }

                _currentTypes.Push(node);

                VisitChildren(members.Where(e =>
                {
                    if (node.IsKind(SyntaxKind.InterfaceDeclaration)/* is InterfaceDeclarationSyntax*/)
                    {
                        if (e is PropertyDeclarationSyntax property)
                            return property.ExpressionBody != null || (property.AccessorList?.Accessors.Any() ?? false);
                        else if (e is MethodDeclarationSyntax method)
                            return method.ExpressionBody != null || method.Body != null;
                    }
                    return !e.IsKind(SyntaxKind.BaseList)/* is not BaseListSyntax*/ && !e.IsKind(SyntaxKind.TypeParameterConstraintClause)/* is not TypeParameterConstraintClauseSyntax*/;
                })/*.OrderBy(e =>
                {
                    //make sure we visit types first, then fields, then properties, constructor and finally methods, so we can have a reference to their TypeSyntax in method body
                    //if (e is BaseTypeDeclarationSyntax)
                        //return int.MaxValue - 3;
                    //if (e is IndexerDeclarationSyntax)
                    //    return int.MaxValue - 2;
                    //if (e is PropertyDeclarationSyntax)
                    //    return int.MaxValue - 2;
                    //if (e is ConstructorDeclarationSyntax)
                    //    return int.MaxValue - 1;
                    //if (e is MethodDeclarationSyntax)
                    //    return int.MaxValue;
                    return 0;
                })*/);

                _currentTypes.Pop();
                writeEpilogue?.Invoke();

                if (!_static)
                {
                    var constructors = members.Where(e => e.IsKind(SyntaxKind.ConstructorDeclaration)/* is ConstructorDeclarationSyntax*/).Cast<ConstructorDeclarationSyntax>()
                        .Where(c => !c.Modifiers.IsStatic());
                    if (node.IsKind(SyntaxKind.StructDeclaration))
                    {
                        //As struct must have a default constructor
                        if (!constructors.Any(e => e.ParameterList.Parameters.Count == 0)) //no default constructor defined, define it
                        {
                            Writer.WriteLine(node, $"/*default valuetype constructor*/ {Constants.DefaultConstructorName}()", true);
                            Writer.WriteLine(node, "{", true);
                            Writer.WriteLine(node, "return this;", true);
                            Writer.WriteLine(node, "}", true);
                        }
                    }
                    else if (node.IsKind(SyntaxKind.ClassDeclaration))
                    {
                        //A class without any constructor has a a default one generated by the compiler
                        if (primaryConstructorParameters == null && !constructors.Any()) //no constructor defined, define default
                        {
                            Writer.WriteLine(node, $"/*default class constructor overload*/ {(constructorCallConvention == ConstructorCallConvention.StaticCall ? "static " : "")}{Constants.DefaultConstructorName}()", true);
                            Writer.WriteLine(node, "{", true);
                            Writer.WriteLine(node, "return this;", true);
                            Writer.WriteLine(node, "}", true);
                        }
                    }
                }


                //Generate clone systems for all struct types, if not existing
                if (typeSymbol.IsValueType &&
                    typeSymbol.EnumUnderlyingType == null/*dont generate for enum type*/ &&
                    !typeSymbol.IsJsPrimitive() &&
                    !typeSymbol.IsStatic &&
                    !typeSymbol.GetMembers().Any(a => a is IMethodSymbol m && m.Name == "Clone"))
                {
                    Writer.WriteLine(node, "Clone($copy)", true);
                    Writer.WriteLine(node, "{", true);
                    Writer.WriteLine(node, "let copy = $copy ?? new this.constructor();", true);
                    //Writer.WriteLine(node, $"{classMetadata.InvocationName}.$ctor.call(copy);", true); //call default constructor generated
                    foreach (var member in typeSymbol.GetMembers())
                    {
                        if (!member.IsStatic && (member is IFieldSymbol || (member is IPropertySymbol p && p.IsAutoProperty())))
                        {
                            if (!member.Name.Contains("<") && !member.Name.Contains("[")) //skip those compiler generated members and indexer
                            {
                                if (member is IFieldSymbol f && f.Type.IsValueType && !f.Type.IsJsPrimitive())
                                {
                                    Writer.WriteLine(node, $"copy.{member.Name} = this.{member.Name}.Clone();", true);
                                }
                                else if (member is IPropertySymbol pr && pr.Type.IsValueType && !pr.Type.IsJsPrimitive())
                                {
                                    Writer.WriteLine(node, $"copy.{member.Name} = this.{member.Name}.Clone();", true);
                                }
                                else
                                    Writer.WriteLine(node, $"copy.{member.Name} = this.{member.Name};", true);
                            }
                        }
                    }
                    Writer.WriteLine(node, "return copy;", true);
                    Writer.WriteLine(node, "}", true);
                }

                bool hasDestuctor = members.Any(a => a.IsKind(SyntaxKind.DestructorDeclaration));

                if (hasDestuctor || CurrentClosure.TypeInitializers.Any(e => !e.Static))
                {
                    Writer.WriteLine(node, "//default member initializer", true);
                    Writer.WriteLine(node, "constructor()", true);
                    Writer.WriteLine(node, $"{{", true);
                    if (!isBootClass && typeSymbol.BaseType != null)
                        Writer.WriteLine(node, $"super();", true);
                    foreach (var init in CurrentClosure.TypeInitializers.Where((e => !e.Static)))
                    {
                        init.Write();
                    }
                    if (hasDestuctor)
                    {
                        Writer.WriteLine(node, $"$.{Constants.FinalizerRegister}(this);", true);
                    }
                    Writer.WriteLine(node, $"}}", true);
                }

                if (CurrentClosure.TypeInitializers.Any(e => e.Static))
                {
                    Writer.WriteLine(node, "//Static Initializer", true);
                    //We cant use javascript default static initializer because we may need to call into other classes not intitialized yet
                    //Writer.WriteLine(node, $"static", true);
                    Writer.WriteLine(node, $"static {Constants.StaticInitializerName}()", true);
                    Writer.WriteLine(node, $"{{", true);
                    foreach (var init in CurrentClosure.TypeInitializers.Where(e => e.Static))
                    {
                        //perform each initialization in its own closure to prevent local variable name clash
                        Writer.WriteLine(node, $"{{", true);
                        OpenClosure(node);
                        init.Write();
                        CloseClosure();
                        Writer.WriteLine(node, $"}}", true);
                    }
                    Writer.WriteLine(node, $"}}", true);
                }

                string? nameGArgs = null;
                var nameArg = (node as TypeDeclarationSyntax)?.TypeParameterList?.Parameters.Select(p => $"({p.Identifier.ValueText}?.FullName??\"\")");
                if (nameArg != null)
                {
                    nameGArgs = " + \"<\" + " + string.Join(" + \",\" + ", nameArg) + " + \">\"";
                }
                Writer.WriteLine(node, $"static get FullName() {{ return \"{_global.ResolveFullTypeName(node)}\"{nameGArgs}; }}", true);

                //if (!_static && !typeSymbol.GetMembers("Is", _global).Any())
                //{
                //    Writer.WriteLine(node, "static $is(value)", true);
                //    Writer.WriteLine(node, "{", true);
                //    Writer.WriteLine(node, $"if (value instanceof {typeMetadata.InvocationName})", true);
                //    Writer.WriteLine(node, "    return true;", true);
                //    if (hasMixin)
                //    {
                //        Writer.WriteLine(node, $"if (Mixin.$is(value))", true);
                //        Writer.WriteLine(node, "    return true;", true);
                //    }
                //    Writer.WriteLine(node, "return false;", true);
                //    Writer.WriteLine(node, "}", true);
                //}

                if (!_static && !typeSymbol.GetMembers("Default", _global).Any())
                {
                    var defaultValue = _global.GetDefaultValue(typeSymbol, true);
                    if (defaultValue != null && defaultValue != "null")
                    {
                        Writer.WriteLine(node, $"static $default() {{ return {defaultValue}; }}", true);
                    }
                }

                Writer.WriteLine(node, $"}}{(!closingClassDeclaration.StartsWith("}") ? closingClassDeclaration : "")}", true);
                if (closingClassDeclaration.StartsWith("}"))
                {
                    Writer.WriteLine(node, closingClassDeclaration, true);
                }
                if (closingClassDeclaration2 != null)
                {
                    Writer.WriteLine(node, closingClassDeclaration2, true);
                }
                //if (!isInnerClass)
                //{
                //    Writer.WriteLine(node, $"}});", true);
                //}

                if (isBootClass) //run the static initializer and constructor immmediately as the runtime wont run it for boot class
                {
                    if (CurrentClosure.TypeInitializers.Any(e => e.Static))
                        Writer.WriteLine(node, $"{_global.GlobalName}.{fullClassName}.{Constants.StaticInitializerName}();", true);
                    if (members.Where(e => e.IsKind(SyntaxKind.ConstructorDeclaration)/* is ConstructorDeclarationSyntax*/).Cast<ConstructorDeclarationSyntax>()
                        .Where(c => c.Modifiers.IsStatic()).Any())
                    {
                        Writer.WriteLine(node, $"{_global.GlobalName}.{fullClassName}.{Constants.StaticConstructorName}();", true);
                    }
                }
            }
            CloseClosure();
        }

        public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
        {
            var field = (IFieldSymbol)_global.GetTypeSymbol(node, this/*, out _, out _*/);
            var siblings = node.Parent!.ChildNodes().Where(e => e.IsKind(SyntaxKind.EnumMemberDeclaration)/* is EnumMemberDeclarationSyntax*/).ToArray();
            Writer.Write(node, $"static {node.Identifier.ValueText}", true);
            if (field.ConstantValue != null)
            {
                Writer.Write(node, $" = ", false);
                Writer.Write(node, field.ConstantValue.ToString(), false);
            }
            else
            {
                Visit(node.EqualsValue);
                if (node.EqualsValue == null)
                {
                    //TODO estimate member value from last EqualsValue
                    Writer.Write(node, $" = {(Array.IndexOf(siblings, node).ToString())}", false);
                }
            }
            Writer.WriteLine(node, $";", false);
            //base.VisitEnumMemberDeclaration(node);
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            WriteTypeDeclaration(node, null, node.Members, writeEpilogue: () =>
            {
                Writer.WriteLine(node, "static get Map()", true);
                Writer.WriteLine(node, "{", true);
                Writer.WriteLine(node, "const map =", true);
                Writer.WriteLine(node, "{", true);
                int ix = 0;
                int nextMemberValue = 0;
                foreach (var member in node.Members)
                {
                    if (ix > 0)
                        Writer.WriteLine(node, ",");
                    Writer.Write(node, "\"", true);
                    Writer.Write(node, member.Identifier.ValueText);
                    Writer.Write(node, "\": ");
                    var field = (IFieldSymbol)_global.GetTypeSymbol(member, this/*, out _, out _*/);
                    if (field.ConstantValue != null)
                    {
                        Writer.Write(node, field.ConstantValue.ToString(), false);
                    }
                    else
                    {
                        Visit(member.EqualsValue?.Value);
                        if (member.EqualsValue == null)
                        {
                            Writer.Write(node, $"{nextMemberValue}", false);
                            nextMemberValue++;
                        }
                        else
                        {
                            //TODO estimate lastmember value from EqualsValue
                        }
                    }
                    Writer.Write(node, "");
                    ix++;
                }
                Writer.WriteLine(node, "");
                Writer.WriteLine(node, "};", true);
                Writer.WriteLine(node, "return map;", true);
                Writer.WriteLine(node, "}", true);
            });
            //base.VisitEnumDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            WriteTypeDeclaration(node, node.ParameterList?.Parameters, node.Members);
            //base.VisitClassDeclaration(node);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            WriteTypeDeclaration(node, node.ParameterList?.Parameters, node.Members);
            //base.VisitStructDeclaration(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            WriteTypeDeclaration(node, node.ParameterList?.Parameters, node.Members);
            //base.VisitInterfaceDeclaration(node);
        }

        public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
        {
            WriteTypeDeclaration(node, node.ParameterList?.Parameters, node.Members);
            //base.VisitRecordDeclaration(node);
        }
    }
}
