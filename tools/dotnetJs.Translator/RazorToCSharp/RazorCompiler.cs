//using Microsoft.AspNetCore.Razor.Language;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.Razor;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;

//namespace BlazorJs.Compiler.Razor
//{
//    public class RazorCompiler
//    {
//        //private IReadOnlyList<TagHelperDescriptor> GetTagHelpers(string tagHelperManifest)
//        //{
//        //    if (!File.Exists(tagHelperManifest))
//        //        return (IReadOnlyList<TagHelperDescriptor>)Array.Empty<TagHelperDescriptor>();
//        //    using (FileStream fileStream = File.OpenRead(tagHelperManifest))
//        //    {
//        //        JsonTextReader reader = new JsonTextReader((TextReader)new StreamReader((Stream)fileStream));
//        //        JsonSerializer jsonSerializer = new JsonSerializer();
//        //        jsonSerializer.Converters.Add((JsonConverter)new RazorDiagnosticJsonConverter());
//        //        //jsonSerializer.Converters.Add((JsonConverter)new TagHelperDescriptorJsonConverter());
//        //        return jsonSerializer.Deserialize<IReadOnlyList<TagHelperDescriptor>>((JsonReader)reader);
//        //    }
//        //}

//        RazorProjectEngine GetEngine(ProjectInfo project, 
//            string textPath, 
//            IReadOnlyList<TagHelperDescriptor> tagHelpers = null)
//        {
//            //var @namespace = compilation.AssemblyName.Replace(".Blazor", "");
//            var pathRelativeToProject = Path.GetDirectoryName(textPath).Replace(project.Path, "").Trim('\\', '/');
//            //var mnamespace = @namespace + "." + pathRelativeToProject.Replace("\\", ".").Replace("/", ".");
//            var razorProject = RazorProjectFileSystem.Create(project.Path);
//            return RazorProjectEngine.Create(
//                RazorConfiguration.Default,
//                razorProject,
//                options =>
//                {
//                    //options.SetNamespace(mnamespace);
//                    options.SetBaseType("IComponentExtension");
//                    options.ConfigureClass((_, node) =>
//                    {
//                        node.ClassName = Path.GetFileName(textPath) + "Extension";

//                        node.Modifiers.Clear();

//                        // Partial to allow extension
//                        node.Modifiers.Add("partial");
//                    });

//                    //options.Features.Add(new DefaultTypeNameFeature());
//                    //options.SetRootNamespace(mnamespace);
//                    //options.Features.Add(new ConfigureRazorCodeGenerationOptions((Action<RazorCodeGenerationOptionsBuilder>)(options =>
//                    //{
//                    //    options.SuppressMetadataSourceChecksumAttributes = !razorSourceGeneratorOptions.GenerateMetadataSourceChecksumAttributes;
//                    //    options.SupportLocalizedComponentNames = razorSourceGeneratorOptions.SupportLocalizedComponentNames;
//                    //})));

//                    //var metadataReferences = new[]
//                    //{
//                    //    compilation.SourceModule.ContainingAssembly?.GetMetadata()?.GetReference()
//                    //}
//                    //.Concat(compilation.SourceModule.ReferencedAssemblySymbols
//                    //.Select(assembly =>
//                    //{
//                    //    return assembly.GetMetadata().GetReference();
//                    //})).Where(t => t != null).ToList();

//                    //options.Features.Add(new DefaultMetadataReferenceFeature()
//                    //{
//                    //    References = metadataReferences
//                    //});
//                    //options.Features.Add(new CompilationTagHelperFeature());
//                    if (tagHelpers != null)
//                    {
//                        options.Features.Add(new RazorStaticTagHelperFeature()
//                        {
//                            TagHelpers = tagHelpers
//                        });
//                    }
//                    options.Features.Add(new DefaultTagHelperDescriptorProvider());
//                    CompilerFeatures.Register(options);
//                    //RazorExtensions.Register(options);
//                    options.SetCSharpLanguageVersion(LanguageVersion.Latest);
//                }
//            );
//        }


//        internal RazorCodeDocument Compile(string textPath, ProjectInfo project)
//        {
//            var razorProject = RazorProjectFileSystem.Create(project.Path);

//            var discoveryEngine = GetEngine(project, textPath);
//            IReadOnlyList<TagHelperDescriptor> tagHelpers = null;/* discoveryEngine.Engine.Features
//                .OfType<ITagHelperFeature>()
//                .Single<ITagHelperFeature>()
//                .GetDescriptors();*/

//            var generationEngine = GetEngine(project, textPath, tagHelpers);
//            var sourceDocument = RazorSourceDocument.Create(File.ReadAllText(textPath), textPath);

//            var codeDocument = generationEngine.Process(
//                sourceDocument,
//                "component",
//                Array.Empty<RazorSourceDocument>(),
//                tagHelpers
//            );
//            return codeDocument;
//        }
//    }
//}
