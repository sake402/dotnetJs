using dotnetJs.Translator.CSharpToJavascript;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace dotnetJs.Translator.CSharpToJavascript
{
    public partial class TranslatorSyntaxVisitor
    {
        string? GetMethodModifier(CSharpSyntaxNode methodNode, SyntaxTokenList modifiers, TypeSyntax? methodReturnType, bool commentAll = false)
        {
            string? modifier = null;
            if (commentAll)
                modifier = "/*";
            if (modifiers.Any(e => e.ValueText == "static"))
            {
                modifier += "static ";
            }
            bool isGeneric = false;
            if (methodNode is MethodDeclarationSyntax mt && mt.Arity > 0)
                isGeneric = true;
            if (methodReturnType != null)
            {
                var returnType = methodReturnType.ToFullString().Trim();
                if (!commentAll)
                    modifier += $"/*";
                modifier += $"{returnType.Replace("/*", "/ *").Replace("*/", "* /")}";
                if (!commentAll)
                    modifier += $"*/ ";
            }
            modifier = modifier?.Trim();
            if (commentAll)
                modifier += "*/";
            //A generic method is implemeted as a factory of function. It cant be async itself, but the factory result can be async
            if (!isGeneric && modifiers.IsAsync())
            {
                modifier += "async ";
            }
            return modifier;
        }

        void WriteMethodGenericArgument(CSharpSyntaxNode node, IMethodSymbol method, Dictionary<ITypeParameterSymbol, ISymbol>? genericTypeSubstitutions = null)
        {
            if (_global.HasAttribute(method, typeof(IgnoreCastAttribute).FullName, this, false, out _))
                return;
            var parameters = method.TypeParameters;
            if (method.Arity == 0 && method.Name == ".ctor")
            {
                parameters = method.ContainingType.TypeParameters;
            }
            if (parameters.Any())
            {
                Writer.Write(node, "(");
                int ix = 0;
                foreach (var t in parameters)
                {
                    if (ix > 0)
                        Writer.Write(node, ", ");
                    if (t is INamedTypeSymbol nt)
                    {
                        Writer.Write(node, nt.ComputeOutputTypeName(_global));
                    }
                    else if (t is IArrayTypeSymbol arr)
                    {
                        Writer.Write(node, arr.ComputeOutputTypeName(_global));
                    }
                    else if (t.Equals(_global.Compilation.DynamicType, SymbolEqualityComparer.Default))
                    {
                        var systemObject = _global.GetTypeSymbol("System.Object", this/*, out _, out _*/);
                        var meta = _global.GetRequiredMetadata(systemObject);
                        Writer.Write(node, meta.OverloadName ?? "System.Object");
                    }
                    else
                    {
                        var subst = genericTypeSubstitutions?.FirstOrDefault(e => e.Key.Name == t.Name).Value;
                        if (subst != null && subst is not ITypeParameterSymbol)
                        {
                            var substSymbol = _global.GetMetadata(subst);
                            Writer.Write(node, substSymbol?.OverloadName ?? t.Name);
                        }
                        else
                        {
                            Writer.Write(node, t.Name);
                        }
                    }
                    ix++;
                }
                Writer.Write(node, ")");
            }
        }

        void WriteSingleMethodInvocationArgument(
            CSharpSyntaxNode node,
            int index,
            CodeNode? arg,
            ITypeSymbol? argType,
            IParameterSymbol parameter,
            MethodOverloadResult overloadResult)
        {
            var substitution = overloadResult.ParameterValueSubstitutions?.GetValueOrDefault(parameter);
            if (substitution != null && substitution.Value.SelectedUnionItem != null && arg != null && arg.IsT0)
            {
                var options = GetExpressionReturnSymbol(arg.AsT0);
                if (options.TypeSyntaxOrSymbol is MemberSymbolOverload overloads)
                {
                    var matched = overloads.Overloads.Where(e => e is IMethodSymbol).FirstOrDefault(v => v.CanConvertTo(substitution.Value.SelectedUnionItem, _global, null, out _) > 0);
                    if (matched != null)
                    {
                        var meta = _global.GetRequiredMetadata(matched);
                        if ((meta.InvocationName ?? meta.OverloadName) != null)
                        {
                            Writer.Write(node, meta.InvocationName ?? meta.OverloadName!);
                            if (!matched.IsStatic)
                                Writer.Write(node, ".bind(this)");
                            return;
                        }
                    }
                }
            }
            if (arg == null && parameter.HasExplicitDefaultValue)
            {
                string? defaultValue = null;
                var vDefaultValue = parameter.ExplicitDefaultValue;
                if (vDefaultValue is bool b)
                {
                    defaultValue = b ? "true" : "false";
                }
                else if (vDefaultValue is string s)
                {
                    defaultValue = "\"" + s + "\"";
                }
                else if (vDefaultValue is char c)
                {
                    defaultValue = ((int)c).ToString();
                }
                else
                {
                    defaultValue = vDefaultValue?.ToString();
                }
                if (defaultValue == null && !parameter.Type.IsNullable(out _) && parameter.Type.IsValueType)
                {
                    defaultValue = $"{parameter.Type.ComputeOutputTypeName(_global)}.default()";
                }
                Writer.Write(node, defaultValue ?? "null");
            }
            else if (arg == null)
            {
                Writer.Write(node, "{");
                Writer.Write(node, index.ToString());
                Writer.Write(node, "}");
            }
            else
            {
                IDisposable? disposeAnonymousTypeDefinition = null;
                if (parameter.Type.IsDelegate(out var delegateReturnType, out var delegateParameterTypes))
                {
                    disposeAnonymousTypeDefinition = CurrentClosure.DefineAnonymousMethodParameterTypes(delegateParameterTypes!);
                }
                if ((parameter.RefKind == RefKind.Out || parameter.RefKind == RefKind.Ref) && arg.IsT0 && arg.AsT0 is ArgumentSyntax aarg && aarg.Expression is DeclarationExpressionSyntax dec && dec.Designation is SingleVariableDesignationSyntax sv)
                {
                    var localSymbol = _global.TryGetTypeSymbol(sv, this/*, out _, out _*/);
                    if (localSymbol != null)
                        CurrentClosure.DefineIdentifierType(sv.Identifier.ValueText, CodeSymbol.From(localSymbol));
                    else
                        CurrentClosure.DefineIdentifierType(sv.Identifier.ValueText, CodeSymbol.From(parameter.Type));
                }
                if (arg.IsT0)
                {
                    var disposable = RegisterTypeInference(arg.AsT0, parameter.Type);
                    WriteVariableAssignment(node, null, parameter, null, arg.AsT0, argType);
                    disposable.Dispose();
                    //Visit(arg);
                    disposeAnonymousTypeDefinition?.Dispose();
                }
                else if (arg.IsT1)
                {
                    arg.AsT1();
                }
            }
        }

        void WriteMethodDeclarationParameters(CSharpSyntaxNode node, SeparatedSyntaxList<ParameterSyntax> parameters)
        {
            int i = 0;
            if (node.Parent.IsKind(SyntaxKind.ExtensionBlockDeclaration))
            {
                var extensionBlock = (ExtensionBlockDeclarationSyntax)node.Parent;
                var extensionParameter = extensionBlock.ParameterList!.Parameters.Single();
                Writer.Write(node, $"/*this {extensionParameter.Type}*/{extensionParameter.Identifier.ValueText}", true);
                i++;
            }
            var symbol = _global.TryGetTypeSymbol(node, this/*, out _, out _*/);
            foreach (var p in parameters/*.Where(e => e.Default == null)*/)
            {
                EnsureImported(p.Type);
                if (symbol is IMethodSymbol method)
                {
                    var parameter = method.Parameters.ElementAt(i);
                    CurrentClosure.DefineIdentifierType(p.Identifier.ValueText, CodeSymbol.From(parameter));
                }
                else if (symbol is IPropertySymbol property)
                {
                    var parameter = property.Parameters.ElementAt(i);
                    CurrentClosure.DefineIdentifierType(p.Identifier.ValueText, CodeSymbol.From(parameter));
                }
                else if (p.Type != null)
                    CurrentClosure.DefineIdentifierType(p.Identifier.ValueText, p.Type, SymbolKind.Parameter);
                if (i > 0)
                    Writer.Write(node, ", ");
                Writer.Write(node, $"/*{string.Join(" ", p.Modifiers.Select(m => m.ValueText))}{(p.Modifiers.Count > 0 ? " " : "")}{p.Type?.ToFullString().Trim()}*/ {Utilities.ResolveIdentifierName(p.Identifier)}");
                i++;
            }
            //if (parameters.Where(e => e.Default != null).Any())
            //{
            //    if (i > 0)
            //        Writer.Write(node, ", ");
            //    int ix = 0;
            //    Writer.Write(node, "{ ");
            //    foreach (var p in parameters.Where(e => e.Default != null))
            //    {
            //        EnsureImported(p.Type);
            //        if (p.Type != null)
            //            CurrentClosure.DefineIdentifierType(p.Identifier.ValueText, p.Type);
            //        if (ix > 0)
            //            Writer.Write(node, ", ");
            //        Writer.Write(node, $"/*{p.Type?.ToFullString().Trim()}*/ {p.Identifier.Text}");
            //        Visit(p.Default);
            //        i++;
            //        ix++;
            //    }
            //    Writer.Write(node, "}");
            //}
        }

        void WriteAdditionalArgument(CSharpSyntaxNode node, CodeNode arg, ref int ix)
        {
            if (arg.IsT0)
            {
                if (ix > 0)
                    Writer.Write(node, ", ");
                Visit(arg.AsT0);
                ix++;
            }
            else if (arg.IsT1)
            {
                if (ix > 0)
                    Writer.Write(node, ", ");
                arg.AsT1();
                ix++;
            }
            //else if (arg.IsT2)
            //{
            //    if (ix > 0)
            //        Writer.Write(node, ", ");
            //    Writer.Write(node, arg.AsT2);
            //    ix++;
            //}
        }

        void WriteMethodInvocationParameter(
            CSharpSyntaxNode node,
            IMethodSymbol? targetMethod,
            TypeArgumentListSyntax? genericArgs,
            IEnumerable<CodeNode>? arguments,
            CodeNode? prefixArguments = null,
            CodeNode? suffixArguments = null,
            MethodOverloadResult overloadResult = default)
        {
            Writer.Write(node, "(");
            //if (node is InvocationExpressionSyntax)
            //{
            //    IEnumerable<GenericNameSyntax> genericNames =
            //    [
            //        .. node.ChildNodes().Where(m => m.IsKind(SyntaxKind.GenericName)/* m is GenericNameSyntax*/).Cast<GenericNameSyntax>(),
            //        .. node.ChildNodes().Where(e=> e is MemberAccessExpressionSyntax).Cast<MemberAccessExpressionSyntax>().Select(m => m.Name).Where(m => m is GenericNameSyntax).Cast<GenericNameSyntax>()
            //    ];
            //    if (genericNames.Any())
            //    {
            //        var genericArguments = string.Join(",", genericNames.Select(a =>
            //        {
            //            return string.Join(", ", a.TypeArgumentList.Arguments.Select(a => Utilities.ResolveTypeName(a)));
            //        }));
            //        Writer.Write(node, genericArguments);
            //        if (arguments?.Arguments.Any() ?? false)
            //            Writer.Write(node, ", ");
            //    }
            //}
            int ix = 0;
            if (prefixArguments != null)
            {
                WriteAdditionalArgument(node, prefixArguments, ref ix);
            }
            if (arguments != null)
            {
                if (targetMethod != null)
                {
                    IEnumerable<IParameterSymbol> parameters = targetMethod.Parameters;
                    if (targetMethod.IsExtensionMethod && prefixArguments != null)
                    {
                        parameters = parameters.Skip(1);
                    }
                    int arg_i = 0;
                    List<CodeNode> remainingArguments = new(arguments);
                    foreach (var parameter in parameters)
                    {
                        var arg = remainingArguments.FirstOrDefault(e => e.IsT0 && e.AsT0 is ArgumentSyntax ar && ar.NameColon?.Name.ToString() == parameter.Name) ?? arguments.ElementAtOrDefault(arg_i);
                        if (arg == null && !parameter.IsParams && !parameter.HasExplicitDefaultValue)
                        {
                            if (targetMethod.Parameters.Count() == arguments.Count() + 1 && parameter.Name == "value" && suffixArguments != null) //if we are writing an indexer, break and write the last parameter supplied as the value
                            {
                                break;
                            }
                            throw new InvalidOperationException($"No argument was supplied for {parameter.Name} in {targetMethod}");
                        }
                        var argSubstitution = overloadResult.ParameterValueSubstitutions?.GetValueOrDefault(parameter);
                        var argType = argSubstitution?.ArgumentType;
                        //parameter.Type.IsDelegate(out var delegateReturnType, out var delegateParameterTypes);
                        //var argType = arg != null ? (ITypeSymbol?)GetTypeSymbol(GetExpressionReturnType(arg.Expression, lamdaParameterTypes: delegateParameterTypes), out _) : null;
                        if (parameter.IsParams && (argType == null || argType.CanConvertTo(parameter.Type, _global, null, out _) <= 0))
                        {
                            if (remainingArguments.Count() == 1) //if the last parameter passed is an array than can convert directly to the target type. dont create another array to wrap it again
                            {
                                var singleParam = remainingArguments.Single();
                                var singleParamType = singleParam.IsT0 ? _global.ResolveSymbol(GetExpressionReturnSymbol(singleParam.AsT0), this/*, out _, out _*/) : null;
                                if (singleParamType?.CanConvertTo(parameter.Type, _global, null, out _) >= 0)
                                {
                                    if (ix > 0)
                                        Writer.Write(node, ", ");
                                    WriteSingleMethodInvocationArgument(node, arg_i, arg, argType, parameter, overloadResult);
                                    //Visit(arg.Expression);
                                    if (arg != null)
                                        remainingArguments.Remove(arg);
                                    break;
                                }
                            }
                            if (ix > 0)
                                Writer.Write(node, ", ");
                            int iip = 0;
                            Writer.Write(node, "[ ", false);
                            foreach (var argument in remainingArguments)
                            {
                                if (iip > 0)
                                    Writer.Write(node, ", ");
                                WriteSingleMethodInvocationArgument(node, arg_i, argument, null, parameter, overloadResult);
                                //Visit(argument.Expression);
                                iip++;
                            }
                            Writer.Write(node, " ]", false);
                            break;
                        }
                        else
                        {
                            if (ix > 0)
                                Writer.Write(node, ", ");
                            WriteSingleMethodInvocationArgument(node, arg_i, arg, argType, parameter, overloadResult);
                            //Visit(arg.Expression);
                            if (arg != null)
                                remainingArguments.Remove(arg);
                        }
                        ix++;
                        arg_i++;
                    }
                }
                else
                {
                    int arg_i = 0;
                    foreach (var arg in arguments.Where(e => (e.IsT0 ? e.AsT0 as ArgumentSyntax : null)?.NameColon == null))
                    {
                        if (ix > 0)
                            Writer.Write(node, ", ");
                        if (arg.IsT0)
                            WriteVariableAssignment(node, null, null, null, arg.AsT0);
                        else
                            arg.AsT1();
                        //Visit(arg);
                        //WriteSingleMethodInvocationArgument(node, arg_i,arg,null);
                        ix++;
                        arg_i++;
                    }
                    if (arguments.Any(e => e.IsT0 && e.AsT0 is ArgumentSyntax ar && ar.NameColon != null))
                    {
                        if (ix > 0)
                            Writer.Write(node, ", ");
                        Writer.Write(node, "{ ");
                        int ix2 = 0;
                        foreach (var arg in arguments.Where(e => e.IsT0 && e.AsT0 is ArgumentSyntax ar && ar.NameColon != null))
                        {
                            if (ix2 > 0)
                                Writer.Write(node, ", ");
                            WriteVariableAssignment(node, null, null, null, arg.AsT0);
                            //Visit(arg);
                            ix2++;
                            ix++;
                        }
                        Writer.Write(node, " }");
                    }
                }
            }
            if (suffixArguments != null)
            {
                WriteAdditionalArgument(node, suffixArguments, ref ix);
            }
            Writer.Write(node, ")");
        }

        List<IMethodSymbol> discriminatedInterfaceMethodImplemented = new List<IMethodSymbol>();
        /// <summary>
        /// if a method is not an explicit interface implementation, but this method implements an interface method though,
        /// we need to create an alias to this method with the interface name
        /// </summary>
        /// <param name="node"></param>
        /// <param name="explicitInterface"></param>
        /// <param name="parameters"></param>
        /// <param name="methodSymbol"></param>
        void TryWriteImplementedMethod(CSharpSyntaxNode node, ExplicitInterfaceSpecifierSyntax? explicitInterface, ParameterListSyntax? parameters, IMethodSymbol? methodSymbol)
        {
            if (explicitInterface == null && methodSymbol != null && methodSymbol.ContainingType.Interfaces.Any())
            {
                if (!methodSymbol.IsExtern && !_global.HasAttribute(methodSymbol, typeof(ExternalAttribute).FullName, this, false, out _) && !_global.HasAttribute(methodSymbol.ContainingSymbol, typeof(ExternalAttribute).FullName, this, false, out _))
                {
                    //find the interfaces that this method implements
                    var implementedMethods = methodSymbol.ContainingType.AllInterfaces
                        .SelectMany(i => i.GetMembers().OfType<IMethodSymbol>())
                        .Where(im => methodSymbol.Equals(methodSymbol.ContainingType.FindImplementationForInterfaceMember(im), SymbolEqualityComparer.Default));
                    var metadata = _global.GetMetadata(methodSymbol);
                    var methodInvocationName = metadata?.InvocationName ?? throw new InvalidOperationException();// Utilities.ResolveMethodName(node);
                    foreach (var implementedMethod in implementedMethods.Except(discriminatedInterfaceMethodImplemented))
                    {
                        if (!implementedMethod.IsExtern && !_global.HasAttribute(implementedMethod, typeof(ExternalAttribute).FullName, this, false, out _) && !_global.HasAttribute(implementedMethod.ContainingSymbol, typeof(ExternalAttribute).FullName, this, false, out _))
                        {
                            OpenClosure(node);
                            Writer.WriteLine(node, $"//Generated explicit method implemetation for {implementedMethod}", true);
                            var symbol = _global.GetRequiredMetadata(implementedMethod);
                            Writer.Write(node, $"{(implementedMethod.IsStatic ? "static " : "")}{symbol.OverloadName}", true);
                            Writer.Write(node, $"(", false);
                            //No need to write method argument explicitly since we use sptread operator on arguments
                            //WriteMethodDeclarationParameters(node, parameters?.Parameters ?? default);
                            Writer.WriteLine(node, $")", false);
                            Writer.WriteLine(node, $"{{", true);

                            //For implemented interface members call that may conflict in name:
                            //eg if a class implement both IComparer<string> and IComparer<char>
                            //the compare implementation methods for both implementation will conflictly be named System$Collections$Generic$IComparer$$$Compare
                            //even though one will receive a string and the other a char
                            //We need to discriminate the one we intend to call by checking the type Ts passed as a last argument to this
                            var conflictingInterfaces = methodSymbol.ContainingType.AllInterfaces.GroupBy(e => e.OriginalDefinition, SymbolEqualityComparer.Default).Where(a => a.Count() > 1);
                            if (conflictingInterfaces.Any())
                            {
                                var conflictingMethods = conflictingInterfaces.SelectMany(g => g.SelectMany(i => i.GetMembers().Where(m => m.OriginalDefinition.Equals(implementedMethod.OriginalDefinition, SymbolEqualityComparer.Default))))
                                    .Cast<IMethodSymbol>();
                                var conflctingTs = conflictingMethods.Select(m => (m.ContainingType, m.ContainingType.TypeArguments)).ToList();
                                if (implementedMethod.ContainingType.TypeKind == TypeKind.Interface && implementedMethod.ContainingType.TypeArguments.Any() && implementedMethod.ContainingType.TypeArguments.All(a => a is INamedTypeSymbol))
                                {
                                    Writer.WriteLine(node, $"let $ts = arguments[arguments.length-1];", true);
                                    foreach (var method in conflictingMethods.Except([implementedMethod]))
                                    {
                                        discriminatedInterfaceMethodImplemented.Add(method);
                                        var ifs = string.Join(" && ", method.ContainingType.TypeArguments.Select((t, i) => $"$ts[{i}] === {t.ComputeOutputTypeName(_global)}"));
                                        Writer.WriteLine(node, $"//Merged and discriminated with method implemetation for {method}", true);
                                        Writer.WriteLine(node, $"if ({ifs})", true);
                                        var implementation = methodSymbol.ContainingType.FindImplementationForInterfaceMember(method)!;
                                        var implementationMetadata = _global.GetRequiredMetadata(implementation);
                                        Writer.WriteLine(node, $"    return {(!methodSymbol.IsStatic ? "this." : "")}{implementationMetadata.InvocationName}(...arguments);", true);
                                    }
                                }
                            }
                            Writer.WriteLine(node, $"{(methodSymbol.ReceiverType != null ? "return " : "")}{(!methodSymbol.IsStatic ? "this." : "")}{methodInvocationName}(...arguments);", true);
                            Writer.WriteLine(node, $"}}", true);
                            CloseClosure();
                        }
                    }
                }
            }
        }
        void WriteMethodBody(BaseMethodDeclarationSyntax node, TypeSyntax? returnType, SeparatedSyntaxList<TypeParameterSyntax>? typeParameters, IEnumerable<ParameterSyntax> parameters, Action? writePrologue = null, Action? writeEpilogue = null)
        {
            if (node.ExpressionBody == null && node.Body == null)
                throw new InvalidOperationException("Cannot write method without a body");
            //TODO: operator methods are not getting their symbols
            var methodSymbol = _global.TryGetTypeSymbol(node, this/*, out _, out _*/);
            if (node.ExpressionBody != null || writePrologue != null)
            {
                Writer.WriteLine(node, "{", true);
            }
            writePrologue?.Invoke();
            bool needClosurePop = false;
            if (CurrentClosure.Syntax != node)
            {
                OpenClosure(node);
                needClosurePop = true;
            }
            DefineParametersInClosure(parameters, methodSymbol);
            if (typeParameters != null)
            {
                int ixx = 0;
                foreach (var tp in typeParameters)
                {
                    if (methodSymbol is IMethodSymbol method)
                    {
                        CurrentClosure.DefineIdentifierType(tp.Identifier.ValueText, CodeSymbol.From(method.TypeParameters.ElementAt(ixx)));
                    }
                    else
                    {
                        CurrentClosure.DefineIdentifierType(tp.Identifier.ValueText, CodeSymbol.From(tp, SymbolKind.Parameter));
                    }
                    ixx++;
                }
            }
            TryWrapInYieldingGetEnumerable(
                node,
                (returnType as GenericNameSyntax)?.TypeArgumentList.Arguments,
                [node.Body ?? (SyntaxNode)node.ExpressionBody!],
                isAsync: node.Modifiers.IsAsync()
            //node.ChildNodes().Where(e => !e.IsKind(SyntaxKind.ThisConstructorInitializer) && !e.IsKind(SyntaxKind.ThisConstructorInitializer) && !e.IsKind(SyntaxKind.BaseConstructorInitializer) && !e.IsKind(SyntaxKind.IdentifierName)/* is not IdentifierNameSyntax*/ && !e.IsKind(SyntaxKind.ParameterList) /*is not ParameterListSyntax */&& !e.IsKind(SyntaxKind.TypeParameterConstraintClause)/* is not TypeParameterConstraintClauseSyntax*/ && !e.IsKind(SyntaxKind.ExplicitInterfaceSpecifier) /*is not ExplicitInterfaceSpecifierSyntax*/).Except(returnType != null ? [returnType] : [])
            );
            if (needClosurePop)
            {
                CloseClosure();
            }
            writeEpilogue?.Invoke();
            if (node.ExpressionBody != null || writePrologue != null)
            {
                Writer.WriteLine(node, "}", true);
            }
        }

        string ConditionalInvokeStart(CSharpSyntaxNode node, CodeNode? lhsExpression)
        {
            var i = ++Writer.CurrentClosure.NameManglingSeed;
            string label = $"$loc{i}";

            Writer.WriteLine(node, $"{_global.GlobalName}.$exp(function()");
            Writer.WriteLine(node, $"{{", true);
            Writer.Write(node, $"const {label} = ", true);
            VisitNode(lhsExpression);
            Writer.WriteLine(node, $";");
            Writer.Write(node, $"return ", true);

            //Writer.Write(node, $"(const {label} = ");
            //Visit(lhsExpression);
            //Writer.Write(node, $", ");
            return label;
        }

        void ConditionalInvokeEnd(CSharpSyntaxNode node)
        {
            Writer.WriteLine(node, $";");
            //Writer.WriteLine(node, $"return $ret;", true);
            Writer.Write(node, $"}})", true);
            //Writer.Write(node, " || null)");
        }

        public void WriteMethodInvocation(
            CSharpSyntaxNode node,
            IMethodSymbol? method,
            ExpressionSyntax? rhsExpression,
            IEnumerable<CodeNode>? parameterArgs,
            CodeNode? lhsExpression,
            ISymbol? lhsSymbol,
            TypeArgumentListSyntax? explicitGenericArgs = null,
            bool conditionallyInvoke = false,
            CodeNode? suffixArguments = null,
            MethodOverloadResult overloadResult = default)
        {
            object[]? conditonalConstructorArgs = null;
            var hasConditional = method != null && _global.HasAttribute(method, typeof(ConditionalAttribute).FullName!, this, false, out conditonalConstructorArgs);
            if (hasConditional)
            {
                var prop = conditonalConstructorArgs![0].ToString()!;
                if (!prop.Equals(_global.Evaluate("Configuration"), StringComparison.InvariantCultureIgnoreCase))
                {
                    return /*true*/;
                }
            }
            if (method != null)
            {
                //var parameterNames = method.Parameters.Select(p => p.Name).ToList();
                var templateAttribute = method.OriginalDefinition.GetTemplateAttribute(_global);
                if (templateAttribute != null)
                {
                    WriteMethodTemplate(node, lhsExpression, lhsSymbol, conditionallyInvoke, method, parameterArgs, templateAttribute, overloadResult, suffixArguments);
                    return /*true*/;
                }
                if (method.IsExtensionMethod)
                {
                    bool calledAsExtensionMethod = lhsSymbol != null && !lhsSymbol.Equals(method.ContainingType, SymbolEqualityComparer.Default);
                    if (calledAsExtensionMethod)
                    {
                        EnsureImported(method.ContainingType);
                        var mlhsLabel = conditionallyInvoke ? ConditionalInvokeStart(node, lhsExpression) : "";
                        var meta = _global.GetRequiredMetadata(method);
                        //Writer.Write(node, meta.InvocationName ?? method.ContainingType.Name);
                        //Writer.Write(node, ".");
                        Writer.Write(node, meta.InvocationName ?? method.Name);
                        //if (method.Arity > 0)
                        //{
                        //    WriteMethodGenericArgument(node, method, overloadResult.GenericTypeSubstitutions);
                        //}
                        WriteMethodInvocationParameter(node, method, explicitGenericArgs, parameterArgs, prefixArguments: /*!conditionallyInvoke ? */lhsExpression /*: mlhsLabel*/, suffixArguments: suffixArguments, overloadResult: overloadResult);
                        if (conditionallyInvoke)
                            ConditionalInvokeEnd(node);
                        return /*true*/;
                    }
                }
            }
            var methodMetadata = method != null && method.ContainingSymbol is not IMethodSymbol/*Currently unable to read local method symbol when collecting symbols*/ ? _global.GetRequiredMetadata(method) : null;
            if ((method?.IsStatic ?? false) || (method?.IsStaticCallConvention(_global) ?? false))
            {
                if (lhsSymbol is ITypeParameterSymbol ttp)
                {
                    Writer.Write(node, ttp.Name);
                    Writer.Write(node, ".");
                    Writer.Write(node, methodMetadata?.OverloadName ?? method.Name);
                }
                else
                {
                    Writer.Write(node, methodMetadata?.InvocationName /*?? methodMetadata?.SimpleName*/ ?? method.Name);
                    if (!method.IsStatic) //static convention call
                    {
                        Writer.Write(node, ".call");
                    }
                }
                if (methodMetadata == null)
                {
                    if (method.Arity > 0)
                    {
                        WriteMethodGenericArgument(node, method, overloadResult.GenericTypeSubstitutions);
                    }
                    //the generic method is called without providing generic argument, use the infered arguments in the genericTypesSubstitutions
                    else if (method.IsGenericMethod && !node.ChildNodes().Any(e => e is GenericNameSyntax))
                    {
                        Writer.Write(node, "(", false);
                        int ix = 0;
                        foreach (var tp in method.TypeParameters)
                        {
                            if (ix > 0)
                                Writer.Write(node, ", ", false);
                            var type = overloadResult.GenericTypeSubstitutions?.GetValueOrDefault(tp);
                            var typeName = type != null ? _global.GetMetadata(type)?.FullName : null;
                            Writer.Write(node, typeName ?? tp.Name);
                            ix++;
                        }
                        Writer.Write(node, ")", false);
                    }
                }
                Action? prefixArgument = !method.IsStatic ? () =>
                {
                    /*static convention call*/
                    if (lhsExpression == null)
                    {
                        Writer.Write(node, "this");
                    }
                    else
                    {
                        VisitNode(lhsExpression);
                    }
                }
                : null;
                WriteMethodInvocationParameter(node, method, explicitGenericArgs, parameterArgs, prefixArguments: prefixArgument, suffixArguments: suffixArguments, overloadResult: overloadResult);
                return /*true*/;
            }
            else
            {
                if (lhsExpression != null)
                {
                    VisitNode(lhsExpression);
                    Writer.Write(node, ".");
                }
                else
                {
                    if (method != null && !method.IsStatic && method.MethodKind != MethodKind.LocalFunction && method.MethodKind != MethodKind.DelegateInvoke)
                    {
                        Writer.Write(node, "this.");
                    }
                }
                if (method?.MethodKind != MethodKind.DelegateInvoke && methodMetadata?.InvocationName != null)
                {
                    Writer.Write(node, methodMetadata.InvocationName);
                }
                else
                {
                    Visit(rhsExpression);
                }
                //Handle implemented interface members call that may conflict in name:
                //eg if a class implement both IComparer<string> and IComparer<char>
                //the compare implementation methods for both implementation will conflictly be named System$Collections$Generic$IComparer$$$Compare
                //even though one will receive a string and the other a char. These two methods are merged into one.
                //We need to discriminate the one we intend to call by passing the type Ts as a last argument to the merged implementation
                //If the called class doesnt implement more than one of these discriminated interface, it simply ignore the T parameter
                if (method != null && method.ContainingType.TypeKind == TypeKind.Interface && method.ContainingType.TypeArguments.Any()/* && method.ContainingType.TypeArguments.All(a => a is INamedTypeSymbol)*/)
                {
                    var existingSuffix = suffixArguments;
                    suffixArguments = (Action)(() =>
                    {
                        int ix = 0;
                        if (existingSuffix != null)
                        {
                            WriteAdditionalArgument(node, existingSuffix, ref ix);
                            Writer.Write(node, ", ");
                        }
                        ix = 0;
                        Writer.Write(node, "[");
                        foreach (var t in method.ContainingType.TypeArguments)
                        {
                            if (ix > 0)
                                Writer.Write(node, ", ");
                            Writer.Write(node, t.ComputeOutputTypeName(_global));
                            ix++;
                        }
                        Writer.Write(node, "]");
                    });
                }
                WriteMethodInvocationParameter(node, method, explicitGenericArgs, parameterArgs, null, suffixArguments: suffixArguments, overloadResult: overloadResult);
                return /*true*/;
            }
        }

        void WriteMethodInvocation(
            CSharpSyntaxNode node,
            string methodFullName,
            Func<IMethodSymbol, bool>? methodFilter = null,
            ITypeSymbol[]? classGenericTypes = null,
            ITypeSymbol[]? methodGenericTypes = null,
            CodeNode? lhsExpression = null,
            IEnumerable<CodeNode>? arguments = null)
        {
            var i = methodFullName.LastIndexOf('.');
            var className = methodFullName.Substring(0, i);
            var methodName = methodFullName.Substring(i + 1);
            var @class = (ITypeSymbol)_global.GetTypeSymbol(className, this);
            if (classGenericTypes != null)
            {
                @class = ((INamedTypeSymbol)@class).Construct(classGenericTypes);
            }
            var method = (IMethodSymbol)@class.GetMembers(methodName).Where(e => e is IMethodSymbol m && (methodFilter?.Invoke(m) ?? true)).Single();
            if (methodGenericTypes != null)
            {
                method = method.Construct(methodGenericTypes);
            }
            WriteMethodInvocation(node,
                method,
                null,
                arguments,
                lhsExpression, @class, null, false);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var parameterArgs = (ArgumentListSyntax?)node.ChildNodes().FirstOrDefault(e => e.IsKind(SyntaxKind.ArgumentList)/* is ArgumentListSyntax*/);
            if (node.Expression is IdentifierNameSyntax nid)
            {
                if (nid.Identifier.ValueText == "nameof")
                {
                    var argName = node.ArgumentList.Arguments.FirstOrDefault();
                    if (argName == null)
                        throw new InvalidOperationException("nameof expects one argument");
                    Writer.Write(node, "\"");
                    Writer.Write(node, argName.ToString());
                    Writer.Write(node, "\"");
                    return;
                }
                //else
                //{
                //    var target = GetIdentifierTypeInScope(nid.Identifier.ValueText);
                //    if (target.TypeSyntaxOrSymbol != null)
                //    {
                //        IMethodSymbol? mmethod = null;
                //        if (target.TypeSyntaxOrSymbol is IMethodSymbol meth)
                //            mmethod = meth;
                //        else if (target.TypeSyntaxOrSymbol is MemberSymbolOverload overloads)
                //            mmethod = overloads.ResolveMethod(this, (TypeArgumentListSyntax?)node.ChildNodes().FirstOrDefault(e => e.IsKind(SyntaxKind.TypeArgumentList)), node.ArgumentList, out _);
                //        //var meta = mmethod != null ? global.ReversedSymbols[mmethod.OriginalDefinition] : null;
                //        WriteMethodInvocation(node, mmethod, node.Expression, null, parameterArgs?.Arguments, null, null, false);
                //        //Writer.Write(node, meta?.InvocationName ?? nid.Identifier.ValueText);
                //        //WriteMethodInvocationParameter(node, mmethod, null, node.ArgumentList);
                //        return;
                //    }
                //}
            }
            var children = node.ChildNodes().Where(e => !e.IsKind(SyntaxKind.ArgumentList)/* is not ArgumentListSyntax*/);
            CodeSymbol lhsType = default;
            TypeArgumentListSyntax? explicitGenericArgs = null;
            ISymbol? lhsSymbol = null;
            string? rhsName = null;
            ExpressionSyntax? lhsExpression = null;
            ExpressionSyntax? rhsExpression = null;
            ISymbol? rhsSymbol = null;
            bool conditionallyInvoke = false;
            //if (conditionally && node.Parent is ConditionalAccessExpressionSyntax cd) //conditional expression was skipped for this 
            //{
            //    var ma = node.Expression as MemberAccessExpressionSyntax;
            //    var mb = node.Expression as MemberBindingExpressionSyntax;
            //    lhsExpression = cd.Expression;
            //    rhsExpression = ma?.Name ?? mb.Name;
            //    rhsName = (ma?.Name ?? mb.Name).Identifier.ValueText;
            //    if ((ma?.Name ?? mb.Name) is GenericNameSyntax gnn)
            //    {
            //        explicitGenericArgs = gnn.TypeArgumentList;
            //    }
            //    lhsType = GetExpressionReturnType(lhsExpression);
            //    if (lhsType.TypeSyntaxOrSymbol != null)
            //    {
            //        lhsSymbol = GetTypeSymbol(lhsType, out _);
            //    }
            //    conditionallyInvoke = true;
            //}
            //else
            if (node.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                lhsExpression = memberAccess.Expression;
                rhsExpression = memberAccess.Name;
                rhsName = memberAccess.Name.Identifier.ValueText;
                if (memberAccess.Name is GenericNameSyntax gn)
                {
                    explicitGenericArgs = gn.TypeArgumentList;
                }
                //CodeType? parentType = null;
                //if (node.Parent is ConditionalAccessExpressionSyntax cd)
                //{
                //    parentType = GetExpressionReturnType(cd.Expression);
                //    conditionallyInvoke = true;
                //}
                lhsType = GetExpressionReturnSymbol(lhsExpression);
                if (lhsType.TypeSyntaxOrSymbol != null)
                {
                    lhsSymbol = _global.ResolveSymbol(lhsType, this/*, out _, out _*/);
                }
                var lhsTypeSymbol = lhsSymbol?.GetTypeSymbol();
                if (lhsTypeSymbol != null)
                {
                    var members = lhsTypeSymbol.GetMembers(rhsName, _global).ToList();
                    if (members.Count() == 1)
                        rhsSymbol = members.Single();
                }
            }
            else if (node.Expression is IdentifierNameSyntax id)
            {
                var boundedTo = GetExpressionBoundTarget(id);
                if (boundedTo.TypeSyntaxOrSymbol is IMethodSymbol smethod)
                {
                    lhsSymbol = smethod!.ContainingSymbol;
                    lhsType = CodeSymbol.From(lhsSymbol);
                }
                else
                {
                    var target = GetIdentifierTypeInScope(id.Identifier.ValueText);
                    if (target.TypeSyntaxOrSymbol != null)
                    {
                        if (target.TypeSyntaxOrSymbol is MemberSymbolOverload overload)
                        {
                            var symbol = overload.ResolveMethod(this, explicitGenericArgs, node.ArgumentList, out _);
                            if (symbol != null)
                            {
                                lhsSymbol = symbol!.ContainingSymbol;
                                lhsType = CodeSymbol.From(lhsSymbol);
                            }
                        }
                        else
                        {
                            var symbol = _global.ResolveSymbol(target, this/*, out var declaringType, out var declaringKind*/);
                            if (symbol is IMethodSymbol)
                            {
                                lhsSymbol = symbol!.ContainingSymbol;
                                lhsType = CodeSymbol.From(lhsSymbol);
                            }
                        }
                    }
                    else
                    {
                        var targetType = node.FindClosestParent<BaseTypeDeclarationSyntax>() ?? throw new InvalidOperationException("Invocation must happen within the scope of a class, albeit static");
                        lhsSymbol = _global.GetTypeSymbol(targetType, this/*, out _, out _*/).GetTypeSymbol();
                        lhsType = CodeSymbol.From(targetType);
                    }
                }
                rhsExpression = id;
                rhsName = id.Identifier.ValueText;
            }
            else if (node.Expression is GenericNameSyntax gn)
            {
                var targetType = node.FindClosestParent<BaseTypeDeclarationSyntax>() ?? throw new InvalidOperationException("Invocation must happen within the scope of a class, albeit static");
                lhsSymbol = _global.GetTypeSymbol(targetType, this).GetTypeSymbol();
                lhsType = CodeSymbol.From(targetType);
                rhsExpression = gn;
                rhsName = gn.Identifier.ValueText;
                explicitGenericArgs = gn.TypeArgumentList;
            }
            else
            {
                rhsExpression = node.Expression;
            }
            IMethodSymbol? method = GetExpressionBoundTarget(node).TypeSyntaxOrSymbol as IMethodSymbol;
            if (method != null)
            {
                int ix = 0;
                foreach (var parameter in method.Parameters)
                {
                    if (parameter.RefKind == RefKind.Out || parameter.RefKind == RefKind.Ref)
                    {
                        var matchingArgument = parameterArgs?.Arguments.FirstOrDefault(e => e.NameColon?.Name.Identifier.ValueText == parameter.Name) ?? parameterArgs?.Arguments.ElementAtOrDefault(ix);
                        if (matchingArgument != null)
                        {
                            if (matchingArgument.Expression is DeclarationExpressionSyntax dv && dv.Designation is SingleVariableDesignationSyntax sv)
                            {
                                var localSymbol = _global.TryGetTypeSymbol(sv, this/*, out _, out _*/);
                                if (localSymbol != null)
                                    CurrentClosure.DefineIdentifierType(sv.Identifier.ValueText, CodeSymbol.From(localSymbol));
                                else
                                    CurrentClosure.DefineIdentifierType(sv.Identifier.ValueText, CodeSymbol.From(parameter.Type));
                            }
                            //else if (matchingArgument.Expression is IdentifierNameSyntax id)
                            //{
                            //    var localSymbol = _global.TryGetTypeSymbol(id, this, out _, out _);
                            //    if (localSymbol != null)
                            //        CurrentClosure.DefineIdentifierType(id.Identifier.ValueText, CodeType.From(localSymbol));
                            //    else
                            //        CurrentClosure.DefineIdentifierType(id.Identifier.ValueText, CodeType.From(parameter.Type));
                            //}
                        }
                        //CurrentClosure.DefineIdentifierType(pv.Value.OutName, p.Type);
                    }
                    ix++;
                }
            }
            MethodOverloadResult overloadResult = default;
            if (lhsSymbol != null && rhsExpression != null && rhsName != null)
            {
                if (lhsType.TypeSyntaxOrSymbol is MemberSymbolOverload overload)
                {
                    method = method ?? overload.ResolveMethod(this, explicitGenericArgs, parameterArgs, out overloadResult);
                }
                else
                {
                    method = method ?? rhsSymbol as IMethodSymbol ?? GetBestOverloadMethod(lhsSymbol.GetTypeSymbol()!, rhsName, explicitGenericArgs, parameterArgs?.Arguments, null, out overloadResult);
                }
                if (method != null)
                {
                    //if (overloadResult.GenericTypeSubstitutions != null)
                    //method = (IMethodSymbol)method.SubstituteGenericType(overloadResult.GenericTypeSubstitutions, global);
                    if (overloadResult.ParameterValueSubstitutions != null)
                    {
                        foreach (var pv in overloadResult.ParameterValueSubstitutions)
                        {
                            if (pv.Value.OutName != null && pv.Value.OutType.TypeSyntaxOrSymbol != null)
                            {
                                CurrentClosure.DefineIdentifierType(pv.Value.OutName, pv.Value.OutType);
                            }
                        }
                    }
                }
            }
            WriteMethodInvocation(node, method, rhsExpression, parameterArgs?.Arguments.Select(a => new CodeNode(a)), lhsExpression, lhsSymbol, explicitGenericArgs, conditionallyInvoke, null, overloadResult);
        }

        bool ShouldExportMethod(CSharpSyntaxNode node)
        {
            if (node.ToString().Contains("Vector512<"))
            {

            }
            if (node is BaseMethodDeclarationSyntax bm && bm.Modifiers.IsExtern())
                return false;
            //var symbol = _global.TryGetTypeSymbol(node, this/*, out _, out _*/);
            //if (symbol is IMethodSymbol methodSymbol)
            //{
            //    ITypeSymbol[] types = [methodSymbol.ReturnType, .. methodSymbol.Parameters.Select(p => p.Type)];
            //    if (types.Any(e => e.IsType("System.Runtime.Intrinsics.Vector512<>", matchGenerics: true) || e.IsType("System.Runtime.Intrinsics.Vector256<>", matchGenerics: true) || e.IsType("System.Runtime.Intrinsics.Vector128<>", matchGenerics: true) || e.IsType("System.Runtime.Intrinsics.Vector64<>", matchGenerics: true)))
            //    {
            //        return false;
            //    }
            //}
            return true;
        }
        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (!ShouldExportMethod(node))
                return;
            //if (node.Modifiers.IsPartial() && node.Body == null && node.ExpressionBody == null)
            //    return;
            //if (node.Body == null && node.ExpressionBody == null)
            //    return;
            var methodSymbol = (IMethodSymbol)OpenClosure(node);// _global.TryGetTypeSymbol(node, this, out _, out _);
            if (_global.LinkTrimOutMethod(methodSymbol))
                return;
            bool external = _global.HasAttribute(methodSymbol, typeof(TemplateAttribute).FullName!, this, false, out _);
            bool export = _global.ShouldExportType(methodSymbol, this);
            if (export && !external && !node.Modifiers.IsAbstract() && (node.Body != null || node.ExpressionBody != null))
            {
                if (methodSymbol.Arity > 0)
                {
                    foreach (var tp in methodSymbol.TypeParameters)
                    {
                        CurrentClosure.DefineIdentifierType(tp.Name, CodeSymbol.From(tp));
                    }
                }
                //CurrentClosure.DefineIdentifierType($"{methodSymbol.Name}{(methodSymbol.IsGenericMethod ? $"<{string.Join(",", Enumerable.Range(1, methodSymbol.Arity).Select(e => ""))}>" : "")}", CodeType.From(methodSymbol));
                var metadata = _global.GetRequiredMetadata(methodSymbol);
                //closures.Push(new CodeBlockClosure(global, semanticModel, node, methodSymbol));
                EnsureImported(node.ReturnType);
                string? modifier = GetMethodModifier(node, node.Modifiers, node.ReturnType);

                if (!methodSymbol.IsStatic && methodSymbol.IsStaticCallConvention(_global))
                {
                    modifier = "static/*conventional*/ " + modifier;
                }

                var methodName = metadata.OverloadName ?? Utilities.ResolveMethodName(node);
                if (node.ExplicitInterfaceSpecifier != null)
                {
                    //var implementingType = GetTypeSymbol(node.ExplicitInterfaceSpecifier.Name);                
                    //methodName = implementingType.ToString()!.Replace(".", "$").Replace("<", "$").Replace(">", "$") + "$" + .;
                }
                //Writer.WriteLine($"{modifier}{node.Identifier.Text.Trim()}({requiredParameters}{(requiredParameters.Length > 0 ? ", " : "")}{optionalParameters})", true);
                Writer.Write(node, $"{modifier} {methodName}", true);
                bool writeGenerics = node.Arity > 0 && (methodSymbol == null || !_global.HasAttribute(methodSymbol, typeof(IgnoreGenericAttribute).FullName, this, false, out _)) && (node.TypeParameterList?.Parameters.Any() ?? false);
                if (writeGenerics)
                {
                    Writer.Write(node, " = (", false);
                    int i = 0;
                    foreach (var p in node.TypeParameterList!.Parameters)
                    {
                        if (i > 0)
                            Writer.Write(node, ", ");
                        Writer.Write(node, p.Identifier.ValueText);
                        i++;
                    }
                    Writer.Write(node, $") => {(node.Modifiers.IsAsync() ? "async " : "")}", false);
                }
                Writer.Write(node, $"(", false);
                WriteMethodDeclarationParameters(node, node.ParameterList.Parameters);
                Writer.Write(node, $")");
                if (writeGenerics)
                {
                    Writer.WriteLine(node, $" =>");
                }
                else
                {
                    Writer.WriteLine(node, $"");
                }
                WriteMethodBody(node, node.ReturnType, node.TypeParameterList?.Parameters, node.ParameterList.Parameters);
            }
            if (!external)
            {
                //if this method is not an explicit interface implementation, but this method implements an interface method,
                //we need to create an alias to this method with the interface name
                TryWriteImplementedMethod(node, node.ExplicitInterfaceSpecifier, node.ParameterList, methodSymbol);
            }
            CloseClosure();
            //base.VisitMethodDeclaration(node);
        }


        public override void VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            if (!ShouldExportMethod(node))
                return;
            if (node.Modifiers.Any(m => m.ValueText == "extern"))
                return;
            if (node.Body == null && node.ExpressionBody == null)
                return;
            //closures.Push(new CodeBlockClosure(global, semanticModel, node));
            string? modifier = GetMethodModifier(node, node.Modifiers, node.ReturnType);
            var operatorSymbol = OpenClosure(node);// _global.GetTypeSymbol(node, this, out _, out _);
            var metadata = _global.GetRequiredMetadata(operatorSymbol);
            Writer.Write(node, $"{modifier} {metadata.OverloadName ?? node.OperatorToken.ValueText.ResolveOperatorMethodName(node.ParameterList.Parameters.Count)}(", true);
            WriteMethodDeclarationParameters(node, node.ParameterList.Parameters);
            Writer.WriteLine(node, $")");
            WriteMethodBody(node, node.ReturnType, null, node.ParameterList.Parameters);
            CloseClosure();
            //closures.Pop();
            //base.VisitOperatorDeclaration(node);
        }

        public override void VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
        {
            if (!ShouldExportMethod(node))
                return;
            EnsureImported(node.Type);
            var symbol = OpenClosure(node);
            var meta = _global.GetRequiredMetadata(symbol);
            //closures.Push(new CodeBlockClosure(global, semanticModel, node));
            string? modifier = GetMethodModifier(node, node.Modifiers, node.Type);
            //var name = Utilities.ResolveTypeName(node.Type, stripGenericName: true);
            //if (node.Type is GenericNameSyntax g)
            //{
            //    name = $"{name}${g.Arity}";
            //}
            Writer.Write(node, $"{modifier} {(meta?.OverloadName ?? "op_Implicit")}(", true);
            WriteMethodDeclarationParameters(node, node.ParameterList.Parameters);
            Writer.WriteLine(node, $")");
            WriteMethodBody(node, node.Type, null, node.ParameterList.Parameters);
            CloseClosure();
            //closures.Pop();
            //base.VisitConversionOperatorDeclaration(node);
        }

        public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            if (!ShouldExportMethod(node))
                return;
            if (node.ExpressionBody == null && node.Body == null)
                return;
            var symbol = _global.TryGetTypeSymbol(node, this/*, out _, out _*/);
            if (symbol != null)
                CurrentClosure.DefineIdentifierType(node.Identifier.ValueText, CodeSymbol.From(symbol));
            OpenClosure(node);
            EnsureImported(node.ReturnType);
            string? modifier = GetMethodModifier(node, node.Modifiers, node.ReturnType, true);
            Writer.Write(node, $"{modifier} function {node.Identifier.ValueText}(", true);
            WriteMethodDeclarationParameters(node, node.ParameterList.Parameters);
            Writer.WriteLine(node, $")");
            //WriteMethodBody(node);
            if (node.ExpressionBody != null)
            {
                Writer.WriteLine(node, "{", true);
            }
            //VisitChildren(node.ChildNodes().Where(e => e is not IdentifierNameSyntax && e is not ParameterListSyntax && e is not TypeParameterConstraintClauseSyntax && e is not ExplicitInterfaceSpecifierSyntax).Except(node.ReturnType != null ? [node.ReturnType] : []));

            TryWrapInYieldingGetEnumerable(
                node,
                (node.ReturnType as GenericNameSyntax)?.TypeArgumentList.Arguments,
                [node.Body ?? (SyntaxNode)node.ExpressionBody!],
                isAsync: node.Modifiers.IsAsync()
            //node.ChildNodes().Where(e => !e.IsKind(SyntaxKind.IdentifierName)/* is not IdentifierNameSyntax*/ && !e.IsKind(SyntaxKind.ParameterList) /*is not ParameterListSyntax */&& !e.IsKind(SyntaxKind.TypeParameterConstraintClause)/* is not TypeParameterConstraintClauseSyntax*/ && !e.IsKind(SyntaxKind.ExplicitInterfaceSpecifier) /*is not ExplicitInterfaceSpecifierSyntax*/).Except(node.ReturnType != null ? [node.ReturnType] : [])
            );

            //base.VisitMethodDeclaration(node);
            if (node.ExpressionBody != null)
            {
                Writer.WriteLine(node, "}", true);
            }
            CloseClosure();
            //base.VisitLocalFunctionStatement(node);
        }

        public override void VisitExtensionBlockDeclaration(ExtensionBlockDeclarationSyntax node)
        {
            if (!ShouldExportMethod(node))
                return;
            List<IDisposable> dispose = new List<IDisposable>();
            if (node.ParameterList != null)
            {
                foreach (var parameter in node.ParameterList.Parameters)
                {
                    var d = CurrentClosure.DefineIdentifierType(parameter.Identifier.ValueText, parameter.Type, SymbolKind.Parameter);
                    dispose.Add(d);
                }
            }
            VisitChildren(node.Members);
            dispose.ForEach(d => d.Dispose());
            //base.VisitExtensionBlockDeclaration(node);
        }

        public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node)
        {
            if (!ShouldExportMethod(node))
                return;
            Writer.WriteLine(node, "$dtor()", true);
            if (node.ExpressionBody != null)
            {
                Writer.WriteLine(node, "{", true);
            }
            base.VisitDestructorDeclaration(node);
            if (node.ExpressionBody != null)
            {
                Writer.WriteLine(node, "}", true);
            }
        }
    }
}
