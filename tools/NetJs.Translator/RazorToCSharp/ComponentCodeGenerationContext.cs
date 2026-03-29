using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using NetJs.Translator;

namespace NetJs.Translator.RazorToCSharp
{
    public class ComponentCodeGenerationContext
    {
        public ComponentCodeGenerationContext(List<string> outStartupCodes, IProject project)
        {
            OutStartupCodes = outStartupCodes;
            Project = project; ;
        }
        public IProject Project { get; }
        public IEnumerable<string>? GlobalUsing { get; set; }
        public string? RazorImports { get; set; }
        public string? RazorFile { get; set; }
        public string? CsFile { get; set; }
        public string Namespace { get; set; } = default!;
        public string ClassName { get; set; } = default!;
        public int RazorSequenceNumber { get; set; }
        public INamedTypeSymbol? ComponentClassSymbol { get; set; }
        public RazorComponent? RazorComponentSymbol { get; set; }
        public CompilationUnitSyntax? ComponentClassCompilationUnit { get; set; }
        public Dictionary<string, ComponentCodeGenerationContext> KnownComponents { get; set; } = default!;
        List<string> OutStartupCodes { get; }
        IEnumerable<IFieldSymbol> GetFieldDeep(INamedTypeSymbol? @class)
        {
            if (@class == null)
                yield break;
            foreach (var p in @class.GetMembers()
            .Where(m => m is IFieldSymbol))
            {
                yield return (IFieldSymbol)p;
            }
            if (@class.BaseType != null && @class.BaseType.Name != "object")
            {
                foreach (var po in GetFieldDeep(@class.BaseType))
                    yield return po;
            }
        }
        IEnumerable<IPropertySymbol> GetPropertiesDeep(INamedTypeSymbol? @class)
        {
            if (@class == null)
                yield break;
            foreach (var p in @class.GetMembers()
            .Where(m => m is IPropertySymbol))
            {
                yield return (IPropertySymbol)p;
            }
            if (@class.BaseType != null && @class.BaseType.Name != "object")
            {
                foreach (var po in GetPropertiesDeep(@class.BaseType))
                    yield return po;
            }
        }
        IEnumerable<IMethodSymbol> GetMethodsDeep(INamedTypeSymbol? @class)
        {
            if (@class == null)
                yield break;
            foreach (var p in @class.GetMembers()
            .Where(m => m is IMethodSymbol))
            {
                yield return (IMethodSymbol)p;
            }
            if (@class.BaseType != null && @class.BaseType.Name != "object")
            {
                foreach (var po in GetMethodsDeep(@class.BaseType))
                    yield return po;
            }
        }
        public IEnumerable<IFieldSymbol>? Fields => GetFieldDeep(ComponentClassSymbol);
        public IEnumerable<IPropertySymbol>? Properties => GetPropertiesDeep(ComponentClassSymbol);
        public IEnumerable<IMethodSymbol>? Methods => GetMethodsDeep(ComponentClassSymbol);
        public IEnumerable<IPropertySymbol>? Templates => Properties?.Where(p => p.SetMethod?.DeclaredAccessibility == Accessibility.Public && p.Type.Name == "RenderFragment");
        public IPropertySymbol? DefaultChildContent => Properties?.FirstOrDefault(p => p.SetMethod?.DeclaredAccessibility == Accessibility.Public && p.Type.Name == "RenderFragment" && p.Name == "ChildContent");
        public bool HasDefaultChildContent => DefaultChildContent != null;

        string GetComponentBaseType()
        {
            var baseT = RazorComponentSymbol?.Inherit?.Name?.ToString() ?? ComponentClassSymbol?.BaseType?.ToString();
            if (baseT == null || baseT == "object")
                baseT = "ComponentBase";
            return baseT;
        }

        public string GenerateCode()
        {
            StringBuilder builder = new StringBuilder();
            List<string> usings = new List<string>();
            void AddUsing(string _using)
            {
                if (!_using.EndsWith(";"))
                {
                    _using += ";";
                }
                if (!usings.Contains(_using))
                {
                    builder.AppendLine(_using);
                    usings.Add(_using);
                }
            }
            AddUsing("using System;");
            AddUsing("using static H5.Core.dom;");
            AddUsing($"using BlazorJs.Core;");
            //AddUsing($"using WebSystem;");
            AddUsing($"using Microsoft.AspNetCore.Components;");
            if (RazorComponentSymbol != null)
            {
                foreach (var _using in RazorComponentSymbol.Usings)
                {
                    AddUsing($"using {_using.Namespace}");
                }
            }
            if (ComponentClassCompilationUnit != null)
            {
                foreach (var _using in ComponentClassCompilationUnit.Usings)
                {
                    AddUsing($"{_using}");
                }
            }
            if (GlobalUsing != null)
            {
                foreach (var _using in GlobalUsing)
                {
                    AddUsing($"{_using}");
                }
            }

            var _namespace = RazorComponentSymbol?.Namespace?.Name ?? ComponentClassSymbol?.ContainingNamespace.ToString() ?? Namespace;
            var _className = ComponentClassSymbol?.Name?.ToString();
            if (_className != null && (ComponentClassSymbol?.IsGenericType ?? false))
            {
                _className = $"{_className}<{string.Join(", ", ComponentClassSymbol.TypeArguments.Select(t => t.Name))}>";
            }
            if (_className == null)
                _className = $"{ClassName}{(RazorComponentSymbol?.TemplateTypes.Any() ?? false ? $"<{string.Join(", ", RazorComponentSymbol.TemplateTypes.Select(t => t.Type))}>" : "")}";

            var assemblyName = Path.GetFileNameWithoutExtension(Project.FullPath);
            var accessSpcifier = assemblyName.Equals("BlazorJs.Core") ? "protected internal" : "protected";
            //var accessSpcifier = (_namespace?.StartsWith("Microsoft.AspNetCore.Components") ?? false) ? "protected internal" : "public";

            string? routeRegistration = null;
            if (RazorComponentSymbol?.Routes.Any() ?? false)
            {
                AddUsing($"using BlazorJs.Core.Components.LiteRouting;");
                string? routeRegistrations = null;
                foreach (var route in RazorComponentSymbol.Routes)
                {
                    var switchRouteParameterTokens = route.Route.Trim().Split('/').Where(t => t.StartsWith("{") && t.EndsWith("}"))
                        .Select(t => t.Substring(1, t.Length - 2))
                        .Select(token => (token, Properties?.FirstOrDefault(p => p.Name == token)))
                        .Where(t => t.Item2 != null)
                        .Select(t => $@"case ""{t.token.ToLower()}"":
                        component.{t.token} = {(t.Item2!.Type.Name == "String" || t.Item2!.Type.Name == "string" || t.Item2!.Type.Name == "System.String" ? "value" : $"ValueConverter.Convert<{t.Item2!.Type}>(value, component.{t.token})")};
                        break;");
                    var parameterSetter = switchRouteParameterTokens.Any() ? $@", routeParameterSetter: (component, name, value) =>
            {{
                switch(name.ToLower())
                {{
                    {string.Join("\r\n", switchRouteParameterTokens)}
                }}
            }}" : null;
                    routeRegistrations += @$"
            RouteTableFactory.Register<{ClassName}>(""{route.Route}""{(RazorComponentSymbol.Layout != null ? $", layout: typeof({RazorComponentSymbol.Layout.Name})" : "")}{parameterSetter});";
                }
                routeRegistration = @$"
        public static void RegisterRoute()
        {{{routeRegistrations}
        }}
";
                OutStartupCodes.Add($"{Namespace}.{ClassName}.RegisterRoute();");
            }

            var injectProperties = ComponentClassSymbol
                ?.GetMembers()
                .Where(p => p is IPropertySymbol || p is IFieldSymbol)
                .Select(property =>
                {
                    var attribute = property.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name.Contains("Inject") ?? false);
                    var arg = attribute?.ConstructorArguments.FirstOrDefault();
                    bool required = true;
                    if (arg.HasValue)
                    {
                        if (arg.Value.Value?.Equals(false) ?? false)
                        {
                            required = false;
                        }
                    }
                    return (property, attribute, required);
                })
                .Where(p => p.Item2 != null);
            var cascadeProperties = ComponentClassSymbol?.GetMembers().Where(p => (p is IPropertySymbol || p is IFieldSymbol) && p.GetAttributes().Any(a => a.AttributeClass?.Name.Contains("CascadingParameter") ?? false));
            ITypeSymbol? GetSymbolType(ISymbol p)
            {
                return (p as IPropertySymbol)?.Type ?? (p as IFieldSymbol)?.Type;
            }
            var injects = RazorComponentSymbol?.Injects.Any() ?? false ? "\r\n" + string.Join("\r\n", RazorComponentSymbol.Injects.Select(i => $"        {i.Type} {i.Name};")) : null;
            string? injectServices = null;
            if ((injectProperties?.Any() ?? false) || (RazorComponentSymbol?.Injects.Any() ?? false))
            {
                injectServices = @$"
        {accessSpcifier} override void InjectServices(IServiceProvider provider)
        {{
{(injectProperties != null ? string.Join("\r\n", injectProperties.Select(p => $"            {p.property.Name} = provider.Get{(p.required ? "Required" : "")}Service<{GetSymbolType(p.property)}>();")) : null)}
{string.Join("\r\n", RazorComponentSymbol?.Injects.Select(p => $"            {p.Name} = provider.GetRequiredService<{p.Type}>();") ?? Enumerable.Empty<string>())}
        }}
";
            }

            string? cascadeParameters = null;

            if (cascadeProperties?.Any() ?? false)
            {
                string? TryQuote(object? s)
                {
                    if (s != null)
                        return $"\"{s}\"";
                    return "null";
                }
                cascadeParameters = @$"
        {accessSpcifier} override void CascadeParameters()
        {{
{string.Join("\r\n", cascadeProperties.Select(p => $"            RequestCascadingParameter<{GetSymbolType(p)}>(e => {p.Name} = e, cascadingParameterName: {TryQuote(p.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name.Contains("CascadingParameter") ?? false)?.ConstructorArguments.FirstOrDefault().Value)});"))}
            base.CascadeParameters();
        }}
";
            }

            string? attributes = null;
            if (RazorComponentSymbol?.Attributes.Any() ?? false)
            {
                attributes = "\r\n" + string.Join("\r\n", RazorComponentSymbol.Attributes.Select(a => $"    {a.Attribute}"));
            }

            string? render = null;
            if (RazorComponentSymbol != null)
            {
                render = @$"
        {accessSpcifier} override void BuildRenderTree(IUIFrame __frame0, object __key = null)
        {{
{string.Join("\r\n", RazorComponentSymbol?.RootNodes.Where(n => !((n as RazorCodeBlock)?.IsCodeBlock ?? false)).Select(r => r.GenerateCode(0, 0, this)) ?? Enumerable.Empty<string>())}
        }}
";
            }

            builder.AppendLine(@$"

namespace {_namespace}
{{{attributes}
    {ComponentClassSymbol?.DeclaredAccessibility.ToString().ToLower() ?? "public"} partial class {_className} : {GetComponentBaseType()}
    {{{injects}{routeRegistration}{injectServices}{cascadeParameters}
{string.Join("\r\n", RazorComponentSymbol?.RootNodes.Where(n => n is RazorCodeBlock cb && cb.IsCodeBlock).Select(r => r.GenerateCode(0, 0, this)) ?? Enumerable.Empty<string>())}{render}
    }}
}}
");
            return builder.ToString();
        }
    }
}
