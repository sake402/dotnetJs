using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetJs.Translator.RazorToCSharp
{
    public class RazorXmlElementAttribute : RazorXmlNode
    {
        public RazorXmlElementAttribute(string name, string? value, RazorXmlNode parent) : base(parent)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public string? Value { get; }

        string TryQuote(string value, IPropertySymbol? property = null)
        {
            if (property == null || property?.Type.Name == "string" || property?.Type.Name == "String" || property?.Type.Name == "System.String")
            {
                return $"\"{value}\"";
            }
            return value;
        }

        public string? GetAttributeValue(IPropertySymbol? assignToProperty = null)
        {
            if (Value == null)
                return null;
            if (Name == "@attributes")
                return Value.StartsWith("@") ? Value.Substring(1) : Value;
            if (Value.StartsWith("@")/* && assignToProperty != null*/)
                return Value.Substring(1);
            if (!Value.Contains('@'))
                return TryQuote(Value, assignToProperty);
            StringBuilder av = new StringBuilder();
            av.Append("$\"");
            for (int i = 0; i < Value.Length; i++)
            {
                if (Value[i] == '@' && i + 1 < Value.Length && Value[i + 1] != '@')
                {
                    av.Append("{");
                    while (i < Value.Length - 1 && Value[i + 1] != ' ')
                    {
                        i++;
                        av.Append(Value[i]);
                    }
                    av.Append("}");
                }
                else
                {
                    av.Append(Value[i]);
                }
            }
            av.Append("\"");
            return av.ToString();
        }

        public override string ToString()
        {
            return $"{Name}=\"{Value}\"";
        }

        bool IsEventCallback(IPropertySymbol property, out INamedTypeSymbol? parameter)
        {
            var str = property.Type.ToString();
            parameter = null;
            if (str == "Microsoft.AspNetCore.Components.EventCallback")
            {
                return true;
            }
            if (str?.StartsWith("Microsoft.AspNetCore.Components.EventCallback") ?? false)
            {
                if (property.Type is INamedTypeSymbol nt && nt.IsGenericType)
                {
                    parameter = (INamedTypeSymbol)nt.TypeArguments[0];
                    return true;
                }
            }
            return false;
        }

        bool IsReturnTask(IMethodSymbol method)
        {
            var str = method.ReturnType.ToString();
            return str == "System.Threading.Tasks.Task"
                || (str?.StartsWith("System.Threading.Tasks.Task") ?? false);
        }

        bool IsReturnTaskT(IMethodSymbol method, out INamedTypeSymbol? mnt)
        {
            mnt = null;
            if (IsReturnTask(method))
            {
                if (method.ReturnType is INamedTypeSymbol nt)
                {
                    mnt = nt;
                    if (nt.IsGenericType)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        string? GetBoundMethodCastType(ComponentCodeGenerationContext context, string name)
        {
            var eventHandlerMethod = (IMethodSymbol?)context.ComponentClassSymbol?.GetMembers().SingleOrDefault(m => m is IMethodSymbol ms && ms.Name == name);
            string? castMethod = null;
            if (eventHandlerMethod != null)
            {
                if (eventHandlerMethod.ReturnsVoid && eventHandlerMethod.Parameters.Count() == 0)
                {
                    castMethod = "(Action)";
                }
                else if (eventHandlerMethod.ReturnsVoid && eventHandlerMethod.Parameters.Count() == 1)
                {
                    castMethod = $"(Action<{eventHandlerMethod.Parameters[0].Type}>)";
                }
                else if (IsReturnTask(eventHandlerMethod) && eventHandlerMethod.Parameters.Count() == 0)
                {
                    castMethod = "(Func<Task>)";
                }
                else if (IsReturnTaskT(eventHandlerMethod, out var nt) && eventHandlerMethod.Parameters.Count() == 0)
                {
                    castMethod = $"(Func<Task<{nt!.TypeArguments[0]}>>)";
                }
                //else if (eventHandlerMethod.Parameters.Count() == 1)
                //{
                //    castMethod = $"(Action<{eventHandlerMethod.Parameters[0]}>)";
                //}
            }
            return castMethod;
        }

        public override string GenerateCode(int tabDepth, int parameterDepth, ComponentCodeGenerationContext context)
        {
            var parent = (RazorXmlElementNode)Parent!;
            if (Name.StartsWith("@bind-"))
            {
                var bindName = Name.Substring(6).Split(':').First();
                if (Name.EndsWith(":event") ||/* Name.EndsWith(":get") ||*/ Name.EndsWith(":set") || Name.EndsWith(":after"))
                {
                    return "";
                }
                bool usingExplicitGetSet = Name.EndsWith(":get");
                string? get = null;
                if (usingExplicitGetSet)
                {
                    get = parent.Attributes.SingleOrDefault(a => a.Name == "@bind-" + bindName + ":get")?.Value;
                }
                else
                {
                    get = Value;
                }
                string? set = null;

                var bindAfter = parent.Attributes.SingleOrDefault(a => a.Name == $"@bind-" + bindName + ":after");
                bool bindAfterMethodIsAsync = false;
                string? bindAfterExpression = null;
                if (bindAfter != null)
                {
                    bindAfterExpression = $"{bindAfter.Value}()";
                    var bindAfterMethod = context.Methods?.SingleOrDefault(m => m.Name == bindAfter.Value);
                    if (bindAfterMethod != null)
                    {
                        bindAfterMethodIsAsync = IsReturnTask(bindAfterMethod);
                        if (bindAfterMethodIsAsync)
                        {
                            bindAfterExpression = "await " + bindAfterExpression;
                        }
                    }
                    else if ((bindAfter.Value?.StartsWith("()=>") ?? false) || (bindAfter.Value?.StartsWith("() =>") ?? false))
                    {
                        var index = bindAfter.Value.IndexOf('>');
                        var methodBody = bindAfter.Value.Substring(index + 1).TrimStart();
                        bindAfterExpression = methodBody;
                        if (methodBody.Contains("await "))
                        {
                            bindAfterMethodIsAsync = true;
                        }
                    }
                }
                if (usingExplicitGetSet)
                {
                    set = parent.Attributes.SingleOrDefault(a => a.Name == "@bind-" + bindName + ":set")?.Value;
                    if (set == null)
                        throw new InvalidOperationException("Using @bind-{}:get or @bind-{}:set requires both get and set pair");
                    if (bindAfter != null)
                    {
                        set = @$"{(bindAfterMethodIsAsync ? "async " : "")}(__value) => 
{GetCodeFormatTabs(tabDepth)}{{
{GetCodeFormatTabs(tabDepth + 1)}{set}();
{GetCodeFormatTabs(tabDepth + 1)}{bindAfterExpression};
{GetCodeFormatTabs(tabDepth)}}}";
                    }
                }
                else
                {
                    if (bindAfter != null)
                    {
                        set = @$"{(bindAfterMethodIsAsync ? "async " : "")}(__value) =>
{GetCodeFormatTabs(tabDepth)}{{
{GetCodeFormatTabs(tabDepth + 1)}{Value} = __value;
{GetCodeFormatTabs(tabDepth + 1)}{bindAfterExpression};
{GetCodeFormatTabs(tabDepth)}}}";
                    }
                    else
                    {
                        set = $"__value => {Value} = __value";
                    }
                }


                var castSetterMethod = GetBoundMethodCastType(context, set);
                var _event = parent.Attributes.SingleOrDefault(a => a.Name == Name + ":event")?.Value ?? "change";
                bool useLocalBoundVariable = castSetterMethod == null;
                var boundValueGetter = useLocalBoundVariable ? $"{GetCodeFormatTabs(tabDepth)}var bindGetValue{parameterDepth} = {get};\r\n" : null;
                var boundSetAttributeValue = useLocalBoundVariable ? $"bindGetValue{parameterDepth}" : get;
                //var eventHandlerMethod = (IMethodSymbol?)context.ComponentClassSymbol?.GetMembers().SingleOrDefault(m => m is IMethodSymbol ms && ms.Name == Value);
                if (parent.IsComponent(context))
                {
                    var propertyExpression = context.KnownComponents[parent.TagName].Properties?.SingleOrDefault(s => s.Name == bindName + "Expression");
                    string? propertyExpressionValue = null;
                    if (propertyExpression != null)
                    {
                        propertyExpressionValue = $"\r\n{GetCodeFormatTabs(tabDepth)}__component{parameterDepth}.{bindName}Expression = () => {Value};";
                    }
                    return @$"{boundValueGetter}{GetCodeFormatTabs(tabDepth)}__component{parameterDepth}.{bindName} = {boundSetAttributeValue};
{GetCodeFormatTabs(tabDepth)}__component{parameterDepth}.{bindName}Changed = EventCallback.Factory.CreateInferred(this, {set}, bindGetValue{parameterDepth});{propertyExpressionValue}";
                }
                else
                {
                    var binder = $"EventCallback.Factory.CreateBinder(this, {castSetterMethod}{set}, {(castSetterMethod == null ? $"bindGetValue{parameterDepth}" : "default")})";
                    return @$"{boundValueGetter}{GetCodeFormatTabs(tabDepth)}__attribute.Set(""{bindName}"", {boundSetAttributeValue});
{GetCodeFormatTabs(tabDepth)}__attribute.Set(""@on{_event}"", {binder});";
                }
                //return @$"{GetCodeFormatTabs(tabDepth)}attribute.Set(""{bindName}"", {get});
                //{GetCodeFormatTabs(tabDepth)}attribute.Set(""@on{_event}"", EventCallback.Create<Event>((Event ev) => 
                //{GetCodeFormatTabs(tabDepth)}{{
                //{GetCodeFormatTabs(tabDepth + 1)}{setter}
                //{GetCodeFormatTabs(tabDepth)}}}, EventCallbackFlags.None));";
            }
            else if (Name.StartsWith("@on"))
            {
                if (Name.EndsWith(":stopPropagation") || Name.EndsWith(":preventDefault"))
                {
                    return "";
                }
                string? flags = null;
                var stopPropagation = parent.Attributes.SingleOrDefault(a => a.Name == Name + ":stopPropagation");
                var preventDefault = parent.Attributes.SingleOrDefault(a => a.Name == Name + ":preventDefault");
                if (stopPropagation != null)
                {
                    if (stopPropagation.Value == "true" || stopPropagation.Value == null)
                    {
                        if (flags != null)
                            flags += " | ";
                        flags += "EventCallbackFlags.StopPropagation";
                    }
                }
                if (preventDefault != null)
                {
                    if (preventDefault.Value == "true" || preventDefault.Value == null)
                    {
                        if (flags != null)
                            flags += " | ";
                        flags += "EventCallbackFlags.PreventDefault";
                    }
                }
                return $"{GetCodeFormatTabs(tabDepth)}__attribute.Set(\"{Name}\", {(Value == null ? "true" : $"EventCallback.Factory.Create(this, {GetBoundMethodCastType(context, Value)}{Value}{(flags != null ? $", {flags}" : "")})")});";
            }
            if (parent.IsComponent(context) && Value != null)
            {
                var component = context.KnownComponents.GetValueOrDefault(parent.TagName);
                var property = component?.Properties?.FirstOrDefault(p => p.Name == Name);
                if (component != null && property != null && IsEventCallback(property, out var parameter))
                {
                    var boundMethod = component.Methods?.SingleOrDefault(s => s.Name == Value);
                    var castMethodType = GetBoundMethodCastType(context, Value);
                    return $"{GetCodeFormatTabs(tabDepth)}__component{parameterDepth}.{Name} = EventCallback.Factory.Create{(parameter != null ? $"<{parameter}>" : "")}(this, {castMethodType}{Value});";
                }
                else if (char.IsLower(Name[0]) || Name[0] == '@' || component != null && property == null)
                {
                    if (Name == "@attributes")
                    {
                        return $"{GetCodeFormatTabs(tabDepth)}__component{parameterDepth}.Set(\"{Name}\", {GetAttributeValue()});";
                    }
                    else
                    {
                        return $"{GetCodeFormatTabs(tabDepth)}__component{parameterDepth}[\"{Name}\"] = {GetAttributeValue()};";
                    }
                }
                else
                {
                    return $"{GetCodeFormatTabs(tabDepth)}__component{parameterDepth}.{Name} = {GetAttributeValue(property)};";
                }
            }
            else
            {
                //var atValue = GetAttributeValue();
                //if (atValue?.Contains('@') ?? false)
                //{
                //    StringBuilder av = new StringBuilder();
                //    av.Append("$\"");
                //    for (int i = 0; i < atValue.Length; i++)
                //    {
                //        if (atValue[i] == '@' && i + 1 < atValue.Length && atValue[i + 1] != '@')
                //        {
                //            av.Append("{");
                //            i++;
                //            while (i < atValue.Length && atValue[i] != ' ')
                //            {
                //                av.Append(atValue[i]);
                //                i++;
                //            }
                //            av.Append("}");
                //        }
                //        else
                //        {
                //            av.Append(atValue[i]);
                //        }
                //    }
                //    av.Append("\"");
                //    return $"{GetCodeFormatTabs(addTabs)}attribute.Set(\"{Name}\", {av});";
                //}
                //else
                //{
                return $"{GetCodeFormatTabs(tabDepth)}__attribute.Set(\"{Name}\"{(Value != null ? ", " : "")}{GetAttributeValue()});";
                //}
            }
        }
    }
}
