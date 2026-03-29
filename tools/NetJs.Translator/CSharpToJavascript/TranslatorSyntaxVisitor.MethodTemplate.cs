using NetJs.Translator.CSharpToJavascript;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetJs.Translator.CSharpToJavascript
{
    public partial class TranslatorSyntaxVisitor
    {
        void WriteMethodTemplate(
            CSharpSyntaxNode node,
            CodeNode? lhsExpression,
            ISymbol? lhsSymbol,
            bool conditionallyInvoke,
            IMethodSymbol? method,
            IEnumerable<CodeNode>? parameterArgs,
            AttributeData templateAttribute,
            MethodOverloadResult overloadResult,
            CodeNode? suffixArguments = null)
        {
            var methodMetadata = method != null ? _global.GetRequiredMetadata(method) : null;
            var parameterNames = method?.Parameters.Select(p => p.Name).ToList();
            var template = templateAttribute.ConstructorArguments.FirstOrDefault().Value?.ToString();
            IEnumerable<CodeNode> arguments = parameterArgs ?? Enumerable.Empty<CodeNode>();
            if (template == null)
            {
                var exp = (arguments.FirstOrDefault().IsT0 ? arguments.FirstOrDefault().AsT0 as ArgumentSyntax : null)?.Expression ?? arguments.FirstOrDefault();
                if (exp.IsT0 && exp.AsT0 is LiteralExpressionSyntax literal)
                {
                    template = literal.ToString();
                    template = template.Replace(@"\\", @"\").Replace(@"\""", "\"");
                    if (template.StartsWith("@"))
                    {
                        template = template.Substring(2, template.Length - 3).Replace("\"\"", "\"");
                    }
                    else
                    {
                        template = template.Substring(1, template.Length - 2); //remove quote;
                    }
                    arguments = arguments.Skip(1);
                }
                else if (exp.IsT0 && exp.AsT0 is InterpolatedStringExpressionSyntax it)
                {
                    template = _global.ResolveInterpolatedExpression(it, this);
                }
                else if (exp.IsT0)
                {
                    var constant = _global.EvaluateConstant(exp.AsT0, this);
                    if (constant.HasValue)
                    {
                        template = constant.Value!.ToString();
                    }
                }
            }
            //Template extension method called using this
            bool isExtensionCall = (method?.IsExtensionMethod ?? false) && lhsSymbol != null && !lhsSymbol.Equals(method.ContainingType, SymbolEqualityComparer.Default);
            var mlhsLabel = conditionallyInvoke ? ConditionalInvokeStart(node, lhsExpression) : "";
            CodeNode? GetArgument(int index)
            {
                if (isExtensionCall)
                {
                    if (index == 0)
                    {
                        if (lhsExpression != null)
                            return lhsExpression;
                        return null;
                    }
                    else
                    {
                        var arg = arguments.ElementAtOrDefault(index - 1);
                        return arg;
                    }
                }
                else
                {
                    var arg = arguments.ElementAtOrDefault(index);
                    return arg;
                }
            }
            void VisitArgument(int index, IParameterSymbol parameter)
            {
                var arg = GetArgument(index);
                var argSubstitution = overloadResult.ParameterValueSubstitutions?.GetValueOrDefault(parameter);
                var argType = argSubstitution?.ArgumentType;
                WriteSingleMethodInvocationArgument(node, index, arg, argType, parameter, overloadResult);
                //if (isExtensionCall)
                //{
                //    if (index == 0)
                //    {
                //        if (conditionallyInvoke)
                //            Writer.Write(node, mlhsLabel);
                //        else
                //            Visit(lhsExpression);
                //    }
                //    else
                //    {
                //        var iarg = arguments.ElementAtOrDefault(index - 1);
                //        WriteSingleMethodInvocationArgument(node, index, iarg, parameter, overloadResult);
                //    }
                //}
                //else
                //{
                //    var iarg = arguments.ElementAtOrDefault(index);
                //    WriteSingleMethodInvocationArgument(node, index, iarg, parameter, overloadResult);
                //}
            }
            Dictionary<string, string> variables = new Dictionary<string, string>();
            if (template != null)
            {
                for (int i = 0; i < template.Length; i++)
                {
                    if (template[i] == '{' && i + 1 < template.Length && template[i + 1] != '{')
                    {
                        var closing = template.IndexOf('}', i);
                        if (closing == -1)
                        {
                            Writer.Write(node, template[i]);
                            continue;
                        }
                        var extract = template.Substring(i + 1, closing - i - 1);
                        if (extract.Contains(' '))
                        {
                            Writer.Write(node, template[i]);
                            continue;
                        }
                        string name = extract;
                        i += extract.Length + 1;
                        var kv = name.Split(':');
                        string? constraint = null;
                        if (kv.Length > 1)
                        {
                            name = kv[0];
                            constraint = kv[1];
                        }
                        if (name == "this")
                        {
                            if (constraint == "!super" && lhsExpression != null && lhsExpression.IsT0 && lhsExpression.AsT0 is BaseExpressionSyntax)
                            {
                                Writer.Write(node, "this");
                            }
                            else
                            {
                                if (conditionallyInvoke)
                                    Writer.Write(node, mlhsLabel);
                                else if (lhsExpression != null)
                                {
                                    if (lhsExpression.IsT0)
                                        WriteVariableAssignment(node, null, lhsSymbol?.GetTypeSymbol(), null, lhsExpression.AsT0, lhsSymbol);
                                    else
                                        lhsExpression.AsT1();
                                    //Visit(lhsExpression);
                                }
                                else
                                {
                                    if (method?.IsStatic ?? false)
                                    {
                                        var containgMetadata = _global.GetRequiredMetadata(method.ContainingType);
                                        Writer.Write(node, containgMetadata.InvocationName ?? "this");
                                    }
                                    else
                                    {
                                        Writer.Write(node, "this");
                                    }
                                }
                            }
                        }
                        else if (name == "global")
                        {
                            Writer.Write(node, _global.GlobalName);
                        }
                        else if (name == "global.")
                        {
                            Writer.Write(node, _global.GlobalName);
                            Writer.Write(node, ".");
                        }
                        //Define a variable in a template by starting it with $v$. Whatever comes after identifies the variable uniquely within the template
                        else if (name.StartsWith(Constants.TemplateVariablePrefix))
                        {
                            if (!variables.TryGetValue(name, out var vName))
                            {
                                var manglingIndex = ++Writer.CurrentClosure.NameManglingSeed;
                                vName = $"$t{name.Substring(Constants.TemplateVariablePrefix.Length)}{manglingIndex}";
                                variables.Add(name, vName);
                                Writer.InsertInCurrentClosure(node, $"let {vName};", true);
                            }
                            Writer.Write(node, vName);
                        }
                        else if (method != null && name == "value" && method.Parameters.Count() == (parameterArgs?.Count() ?? 0) + 1 && suffixArguments != null) //if we are writing an indexer, suffix is value
                        {
                            int ix = 0;
                            WriteAdditionalArgument(node, suffixArguments, ref ix);
                        }
                        else if (method != null && name == "value" && method.Parameters.Count() == 0 && suffixArguments != null) //if we are writing a property setter
                        {
                            int ix = 0;
                            WriteAdditionalArgument(node, suffixArguments, ref ix);
                        }
                        else if (method != null && int.TryParse(name, out var index))
                        {
                            IParameterSymbol parameter;
                            if (index >= method.Parameters.Length && method.Parameters.Last().IsParams)
                            {
                                parameter = method.Parameters.Last();
                            }
                            else
                            {
                                parameter = method.Parameters.ElementAt(index);
                            }
                            VisitArgument(index, parameter);
                        }
                        else
                        {
                            bool hasStarModifier = name.StartsWith("*");
                            if (hasStarModifier)
                                name = name.Substring(1);
                            bool hasQualifier = name.Contains(":");
                            string? qualifier = null;
                            if (hasQualifier)
                            {
                                var mkv = name.Split(':');
                                name = mkv[0];
                                qualifier = mkv[1];
                            }
                            if (qualifier == "raw")
                            {

                            }
                            if (method != null)
                            {
                                if (method.IsGenericMethod)
                                {
                                    var targ = method.OriginalDefinition.TypeParameters.Select((t, i) => (t, i)).FirstOrDefault(t => t.t.Name == name);
                                    if (targ.t?.Name != null)
                                    {
                                        Writer.Write(node, method.TypeArguments.ElementAt(targ.i).ComputeOutputTypeName(_global));
                                        continue;
                                    }
                                }
                                if (method.ContainingType.IsGenericType)
                                {
                                    var targ = method.ContainingType.OriginalDefinition.TypeParameters.Select((t, i) => (t, i)).FirstOrDefault(t => t.t.Name == name);
                                    if (targ.t?.Name != null)
                                    {
                                        Writer.Write(node, method.ContainingType.TypeArguments.ElementAt(targ.i).ComputeOutputTypeName(_global));
                                        continue;
                                    }
                                }
                                if (method.IsGenericMethod && overloadResult.GenericTypeSubstitutions != null)
                                {
                                    var tParameter = overloadResult.GenericTypeSubstitutions.FirstOrDefault(e => e.Key.Name == name).Value;
                                    if (tParameter != null)
                                    {
                                        Writer.Write(node, tParameter.ComputeOutputTypeName(_global));
                                        continue;
                                    }
                                }
                            }
                            IParameterSymbol? parameter = null;
                            var parameterIndex = parameterNames != null ? parameterNames.IndexOf(name) : -1;
                            if (method != null && parameterIndex >= 0)
                            {
                                parameter = method.Parameters.ElementAt(parameterIndex);
                            }
                            int argCount = arguments.Count();
                            if (isExtensionCall)
                            {
                                argCount++;
                            }
                            if (parameter == null) //reconstruct it as is
                            {
                                Writer.Write(node, "{");
                                Writer.Write(node, extract);
                                Writer.Write(node, "}");
                            }
                            else
                            {
                                if (parameter.IsParams)
                                {
                                    var remainingParams = arguments.Skip(parameterIndex - (isExtensionCall ? 1 : 0));
                                    if (remainingParams.Count() == 1) //if the last parameter passed is an array than can convert directly to the target type. dont create another array to wrap it again
                                    {
                                        var singleParam = remainingParams.Single();
                                        var singleParameType = singleParam.IsT0 ? _global.ResolveSymbol(GetExpressionReturnSymbol(singleParam.AsT0), this/*, out _, out _*/)?.GetTypeSymbol() : null;
                                        if (singleParameType?.CanConvertTo(parameter.Type, _global, null, out _) >= 0)
                                        {
                                            VisitArgument(parameterIndex, parameter);
                                            continue;
                                        }
                                    }
                                    //if (hasStarModifier && remainingParams.Count() == 1)
                                    //{
                                    //    WriteSingleMethodInvocationArgument(node, parameterIndex, remainingParams.Single(), null, parameter, overloadResult);
                                    //    //Visit(remainingParams.Single());
                                    //}
                                    //else
                                    {
                                        Writer.Write(node, "[ ", false);
                                        int ix = 0;
                                        foreach (var remaining in remainingParams)
                                        {
                                            if (ix > 0)
                                                Writer.Write(node, ", ");
                                            WriteSingleMethodInvocationArgument(node, parameterIndex, remaining, null, parameter, overloadResult);
                                            //Visit(remaining);
                                            ix++;
                                        }
                                        Writer.Write(node, " ]", false);
                                    }
                                }
                                else
                                {
                                    VisitArgument(parameterIndex, parameter);
                                }
                            }
                        }
                    }
                    else
                    {
                        Writer.Write(node, template[i]);
                        if (template[i] == '{' && i + 1 < template.Length && template[i + 1] == '{')
                            i++;
                        if (template[i] == '}' && i + 1 < template.Length && template[i + 1] == '}')
                            i++;
                        if (template[i] == '\n' && i + 1 < template.Length && template[i + 1] == '\t')
                        {
                            Writer.Write(node, "", true);
                            i++;
                        }
                    }
                }
            }
            else
            {
                var fn = templateAttribute.NamedArguments.FirstOrDefault(f => f.Key == "Fn").Value.Value?.ToString();
                if (fn != null)
                {
                    Writer.Write(node, fn);
                    Writer.Write(node, "(");
                    VisitNode(lhsExpression);
                    Writer.Write(node, ")");
                }
            }
            if (conditionallyInvoke)
                ConditionalInvokeEnd(node);
        }
    }
}
