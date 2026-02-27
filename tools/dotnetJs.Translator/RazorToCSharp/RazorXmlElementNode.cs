using Microsoft.CodeAnalysis;

namespace dotnetJs.Translator.RazorToCSharp
{
    public class RazorXmlElementNode : RazorXmlHasChildrenNode
    {
        public RazorXmlElementNode(string tagName, ReadOnlyMemory<char> raw, RazorXmlNode? parentNode) : base(parentNode)
        {
            TagName = tagName;
            Raw = raw;
        }

        public ReadOnlyMemory<char> Raw { get; }
        public string TagName { get; }

        public bool IsComponent(ComponentCodeGenerationContext context)
        {
            return char.IsUpper(TagName[0]) && context.KnownComponents.ContainsKey(TagName);
        }

        bool IsGenericTypeParameter(ComponentCodeGenerationContext context, RazorXmlElementAttribute attribute)
        {
            if (IsComponent(context))
            {
                var referencedComponent = context.KnownComponents[TagName];
                var typeParameters = referencedComponent.ComponentClassSymbol?.TypeParameters;
                if (typeParameters != null)
                {
                    return typeParameters.Value.Any(g => attribute.Name == g.Name);
                }
            }
            return false;
        }
        public IEnumerable<RazorXmlElementAttribute> Attributes => Children.OfType<RazorXmlElementAttribute>();

        public IEnumerable<RazorXmlElementAttribute> GetTypeParameterAttributes(ComponentCodeGenerationContext context)
        {
            return Children.OfType<RazorXmlElementAttribute>().Where(e => IsGenericTypeParameter(context, e));
        }

        public IEnumerable<RazorXmlElementAttribute> GetValueParameterAttributes(ComponentCodeGenerationContext context)
        {
            return Children.OfType<RazorXmlElementAttribute>().Where(e => !IsGenericTypeParameter(context, e));
        }

        public IEnumerable<RazorXmlNode> UITemplateContentNodes => Children.Where(c => c is not RazorXmlElementAttribute);


        public override string ToString()
        {
            return $"{ToStringFormatTabs}<{TagName} {string.Join(" ", Attributes.Select(a => a.ToString()))}>\r\n{base.ToString()}\r\n{ToStringFormatTabs}</{TagName}>";
        }

        public RazorXmlElementAttribute? GetAttributes()
        {
            return Attributes.SingleOrDefault(a => a.Name == "@attributes");
        }

        public RazorXmlElementAttribute? GetKeyed()
        {
            return Attributes.SingleOrDefault(a => a.Name == "@key");
        }

        public RazorXmlElementAttribute? GetRefed()
        {
            return Attributes.SingleOrDefault(a => a.Name == "@ref");
        }

        bool? canuseMarkup;
        private bool CanUseMarkup(ComponentCodeGenerationContext context)
        {
            if (canuseMarkup != null)
                return canuseMarkup.Value;
            if (IsComponent(context))
            {
                canuseMarkup = false;
                return false;
            }
            if (!Attributes.Any(a => a.Name.StartsWith("@")))
            {
                if (Attributes.All(a => !(a.Value?.Contains("@") ?? false)))
                {
                    if (UITemplateContentNodes.All(e => e is RazorTextNode || e is RazorXmlElementNode element && !element.IsComponent(context) && element.CanUseMarkup(context)))
                    {
                        canuseMarkup = true;
                        return true;
                    }
                }
            }
            canuseMarkup = false;
            return false;
        }

        public override string GenerateCode(int tabDepth, int parameterDepth, ComponentCodeGenerationContext context)
        {
            var keyed = GetKeyed();
            var refed = GetRefed();
            if (!IsComponent(context))
            {
                if (TagName == "text" || TagName == "ktext" || TagName == "itext") //the <text> is just a pseudo, we wont generate a code for it
                {
                    var ret = string.Join("\r\n", UITemplateContentNodes.Select(a => a.GenerateCode(tabDepth + 1, parameterDepth, context)));
                    if (keyed != null)
                    {
                        return @$"{GetCodeFormatTabs(tabDepth)}__frame{parameterDepth}.Frame((__frame{parameterDepth + 1}, __key{parameterDepth + 1}) =>
{GetCodeFormatTabs(tabDepth)}{{
{ret}
{GetCodeFormatTabs(tabDepth)}}}{(keyed?.Value != null || parameterDepth != 0 ? $", key: {keyed?.Value ?? $"__key{parameterDepth}"}" : "")}, sequenceNumber: {context.RazorSequenceNumber++});";
                    }
                    return ret;
                }
                else
                {
                    if (CanUseMarkup(context))
                    {
                        return $"{GetCodeFormatTabs(tabDepth)}__frame{parameterDepth}.Markup(\"{RazorUtility.Escape(Raw.Span)}\"{(parameterDepth != 0 ? $", key: __key{parameterDepth}" : "")}, sequenceNumber: {context.RazorSequenceNumber++});";
                    }
                    else
                    {
                        string? childContents = "null";
                        if (UITemplateContentNodes.Count() > 0)
                        {
                            var cc = string.Join("\r\n", UITemplateContentNodes.Select(a => a.GenerateCode(tabDepth, parameterDepth + 1, context)).Where(a => !string.IsNullOrEmpty(a)));
                            childContents = $@"(__frame{parameterDepth + 1}, __key{parameterDepth + 1}) =>
{GetCodeFormatTabs(tabDepth)}{{
{cc}
{GetCodeFormatTabs(tabDepth)}}}";
                        }
                        string? attributes = "null";
                        var mAttributes = Attributes.Where(a => a.Name != "@key" && a.Name != "@ref");
                        if (mAttributes.Any())
                        {
                            var ats = string.Join("\r\n", mAttributes.Select(a => a.GenerateCode(tabDepth, parameterDepth + 1, context)).Where(a => !string.IsNullOrEmpty(a)));
                            attributes = $@"(ref UIElementAttribute __attribute) =>
{GetCodeFormatTabs(tabDepth)}{{
{ats}
{GetCodeFormatTabs(tabDepth)}}}";
                        }
                        return @$"{GetCodeFormatTabs(tabDepth)}{(refed != null ? $"{refed.Value} = " : "")}__frame{parameterDepth}.Element(""{TagName}"", {attributes}, {childContents}{(keyed?.Value != null || parameterDepth != 0 ? $", key: {keyed?.Value ?? $"__key{parameterDepth}"}" : "")}, sequenceNumber: {context.RazorSequenceNumber++});";
                    }
                }
            }
            else
            {
                string fullComponentName = TagName;
                string? properties = "";
                var referencedComponent = context.KnownComponents[TagName];
                var namedTemplates = UITemplateContentNodes.Where(c =>
                c is RazorXmlElementNode node &&
                char.IsUpper(node.TagName[0]) &&
                (referencedComponent.Properties?.Any(p => p.Name == node.TagName) ?? false)).Cast<RazorXmlElementNode>();
                var usingNamedTemplates = namedTemplates.Any();
                bool hasRenderFragmentGenericTemplate = referencedComponent.Properties?.Any(property =>
                {
                    var pType = property.Type;
                    return pType is INamedTypeSymbol nt && nt.IsGenericType && nt.Name.StartsWith("RenderFragment");
                }) ?? false;
                string? componentParameters = string.Join("\r\n", GetValueParameterAttributes(context).Where(attribute =>
                {
                    if (hasRenderFragmentGenericTemplate && attribute.Name == "Context")
                    {
                        return false;
                    }
                    return true;
                }).Select(t => t.GenerateCode(tabDepth, parameterDepth, context)));
                //bool HasNamedContext(RazorXmlElementNode? declaration)
                //{
                //    return declaration?.Attributes.Any(a => a.Name == "Context") ?? false;
                //}
                string? TryRenderContext(RazorXmlElementNode? declaration, IPropertySymbol? property)
                {
                    var pType = (INamedTypeSymbol?)property?.Type;
                    if ((pType?.IsGenericType ?? false) && pType.Name.StartsWith("RenderFragment"))
                    {
                        var contextName =
                            declaration?.Attributes.FirstOrDefault(a => a.Name == "Context")?.Value ??
                            Attributes.FirstOrDefault(a => a.Name == "Context")?.Value ??
                            "context";
                        return $"({contextName}) => ";
                    }
                    return null;
                }
                if (usingNamedTemplates)
                {
                    componentParameters += "\r\n" + string.Join("\r\n", namedTemplates.Select(namedTemplate =>
$@"{GetCodeFormatTabs(tabDepth + 1)}__component{parameterDepth}.{namedTemplate.TagName} = {TryRenderContext(namedTemplate, referencedComponent.Properties?.FirstOrDefault(p => p.Name == namedTemplate.TagName))}(__frame{parameterDepth + 1}, __key{parameterDepth + 1}) =>
{GetCodeFormatTabs(tabDepth + 1)}{{
{string.Join("\r\n", namedTemplate.Children.Where(c => c is not RazorXmlElementAttribute att || att.Name != "Context").Select(s => s.GenerateCode(tabDepth + 1, parameterDepth + 1, context)))}
{GetCodeFormatTabs(tabDepth + 1)}}};"));
                }
                else if (referencedComponent.HasDefaultChildContent && UITemplateContentNodes.Any())
                {
                    componentParameters += @$"
{GetCodeFormatTabs(tabDepth + 1)}__component{parameterDepth}.ChildContent = {TryRenderContext(this, referencedComponent.DefaultChildContent)}(__frame{parameterDepth + 1}, __key{parameterDepth + 1}) =>
{GetCodeFormatTabs(tabDepth + 1)}{{
{string.Join("\r\n", UITemplateContentNodes.Select(e => e.GenerateCode(tabDepth + 1, parameterDepth + 1, context)))}
{GetCodeFormatTabs(tabDepth + 1)}}};";
                }
                else if (UITemplateContentNodes.Any())
                {
                    componentParameters += @$"
{GetCodeFormatTabs(tabDepth + 1)}__component{parameterDepth}[""ChildContent""] = (RenderFragment)((__frame{parameterDepth + 1}, __key{parameterDepth + 1}) =>
{GetCodeFormatTabs(tabDepth + 1)}{{
{string.Join("\r\n", UITemplateContentNodes.Select(e => e.GenerateCode(tabDepth + 1, parameterDepth + 1, context)))}
{GetCodeFormatTabs(tabDepth + 1)}}});";
                }
                //bool parentIsElement = !(((RazorXmlElementNode?)Parent)?.IsComponent ?? false);
                //var ats = string.Join("\r\n", Attributes.Select(a => $"attribute.{a.Name} = {a.AttributeValue};"));
                //bool usesDefaultChildContent = context.HasDefaultChildContent && Children.Any(c =>  //use ChildContent if
                //c is RazorCodeBlock ||  //there is a code block
                //c is RazorTextBaseNode || //there is a text node
                //(c is RazorXmlElementNode el && !el.IsComponent) //or there is an html element inside this component
                //);
                //if (!usesDefaultChildContent)
                //{
                //    if (ContentNodes.Any())
                //}
                //var elementChildContents = Children.Where(c => (c is not RazorXmlElementNode && c is not RazorInlineCodeBlock) || (c is RazorXmlElementNode el && char.IsLower(el.TagName[0])));
                //var templatesParameters = Children.Where(c => c is RazorXmlElementNode el && char.IsUpper(el.TagName[0])).Cast<RazorXmlElementNode>();
                //if (elementChildContents.Any() && templatesParameters.Any())
                //{
                //    throw new InvalidOperationException("Mixing ChildContent and named Frgament is not allowed");
                //}
                //                    if (usesDefaultChildContent)
                //                    {
                //                        componentParameters = string.Join("\r\n", Attributes.Select(t => t.GenerateCode(depth, context)));
                //                        if (TemplateContentNodes.Any())
                //                        {
                //                            componentParameters += @$"
                //{GetCodeFormatTabs(depth)}    component.ChildContent = (frame{depth + 1}, key{depth + 1}) =>
                //{GetCodeFormatTabs(depth)}    {{
                //{string.Join("\r\n", TemplateContentNodes.Select(e => e.GenerateCode(depth + 1, context)))}
                //{GetCodeFormatTabs(depth)}    }};";
                //                        }
                //                    }
                //                    else if (templatesParameters.Any())
                //                    {
                //                        componentParameters = string.Join("\r\n", templatesParameters.Select(t =>
                //$@"{GetCodeFormatTabs(depth + 1)}component.{t.TagName} = (frame{depth + 1}, key{depth + 1}) =>
                //{GetCodeFormatTabs(depth + 1)}{{
                //{t.GenerateCode(depth + 1, context)}
                //{GetCodeFormatTabs(depth + 1)}}};"));
                //                    }
                properties = !string.IsNullOrEmpty(componentParameters) ? $@"(__component{parameterDepth}) =>
{GetCodeFormatTabs(tabDepth)}{{
{componentParameters}
{GetCodeFormatTabs(tabDepth)}}}" : "null";
                if (referencedComponent.ComponentClassSymbol?.IsGenericType ?? false)
                {
                    var typeParameters = referencedComponent.ComponentClassSymbol.TypeParameters;
                    var gType = Attributes.Where(a => a.Name == "T");
                    fullComponentName = $"{TagName}<{string.Join(", ", typeParameters.Select(g => GetTypeParameterAttributes(context).Single(a => a.Name == g.Name).Value))}>";
                }
                return $@"{GetCodeFormatTabs(tabDepth)}{(refed != null ? $"{refed.Value} = " : "")}__frame{parameterDepth}.Component<{fullComponentName}>({properties}{(keyed?.Value != null || parameterDepth != 0 ? $", key: {keyed?.Value ?? $"__key{parameterDepth}"}" : "")}, sequenceNumber: {context.RazorSequenceNumber++});";
            }
        }
    }
}
