//using Microsoft.CodeAnalysis;
//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Diagnostics;
//using System.Text;

//namespace BlazorJs.Compiler
//{
//    [Generator]
//    public class RazorGenerator : IIncrementalGenerator
//    {
//        public void Initialize(IncrementalGeneratorInitializationContext context)
//        {
//            var syntaxes = context.SyntaxProvider.CreateSyntaxProvider(DoFilter, (syntax, cancellation) => syntax);
//            var text = context.AdditionalTextsProvider.Collect();
//            var compileAndSyntax = context.CompilationProvider.Combine(syntaxes.Collect()).Combine(context.AdditionalTextsProvider.Collect());
//            context.RegisterSourceOutput(compileAndSyntax, Execute);
//        }

//        protected bool DoFilter(SyntaxNode node, CancellationToken token)
//        {
//            Debugger.Launch();
//            return false;
//        }

//        public void Execute(SourceProductionContext source,
//            ((Compilation compilation, ImmutableArray<GeneratorSyntaxContext> syntaxes) syntax, ImmutableArray<AdditionalText> texts) context)
//        {
//            var razorFiles = context.texts.Where(t => t.Path.EndsWith(".razor"));
//            Debugger.Launch();
//        }
//    }
//}
