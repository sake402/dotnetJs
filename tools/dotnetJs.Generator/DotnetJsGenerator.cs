using dotnetJs.Translator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;

namespace dotnetJs.Generator
{
    [Generator]
    public class DotnetJsGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var syntaxes = context.SyntaxProvider.CreateSyntaxProvider(FilterNode, (syntax, cancellation) => syntax);
            var text = context.AdditionalTextsProvider.Collect();
            //var projectInfo = context
            //.AnalyzerConfigOptionsProvider
            //.Select((config, _) =>
            //{
            //    config.GlobalOptions.TryGetValue($"build_property.TargetFramework", out var tfw);
            //    config.GlobalOptions.TryGetValue($"build_property.RootNamespace", out var rns);
            //    config.GlobalOptions.TryGetValue($"build_property.AssemblyName", out var asn);
            //    config.GlobalOptions.TryGetValue($"build_property.ProjectDir", out var prd);
            //    return new Project
            //    {
            //        AssemblyName = asn,
            //        Namespace = rns,
            //        Type = tfw,
            //        Path = prd?.Trim('/', '\\')
            //    };
            //});
            var compileAndSyntax = context.CompilationProvider
                .Combine(syntaxes.Collect())
                .Combine(context.AdditionalTextsProvider.Collect());
            context.RegisterSourceOutput(compileAndSyntax, Execute);
        }

        bool FilterNode(SyntaxNode node, CancellationToken token)
        {
            return true;
        }

        void Execute(SourceProductionContext source,
            ((Compilation compilation, ImmutableArray<GeneratorSyntaxContext> syntaxes) syntax, ImmutableArray<AdditionalText> texts) context)
        {
            Debugger.Launch();
            var wProject = Project.GetProjectDefinition((CSharpCompilation)context.syntax.compilation, context.texts);
            Translator.Translator.Build(wProject, new ProjectOutputProvider(wProject));
        }
    }
}
