using Microsoft.CodeAnalysis;
using System.IO;
using System.Xml.Linq;

namespace NetJs.Translator.CSharpToJavascript
{

    public class ReflectionMetadataBuilder
    {
        GlobalCompilationVisitor global;
        string[]? resxFiles;

        static T[]? NullIfEmpty<T>(T[]? value)
        {
            if (value == null)
                return null;
            if (value.Length == 0)
                return null;
            return value;
        }

        public ReflectionMetadataBuilder(GlobalCompilationVisitor global, string[]? resxFiles)
        {
            this.global = global;
            this.resxFiles = resxFiles;
        }

        //static string RemoveGlobal(string? value)
        //{
        //    if (value?.StartsWith("global::") ?? false)
        //        return value.Substring(8);
        //    return value!;
        //}

        uint assemblyHandle;
        string[] typeNames = default!;
        ReflectionHandleModel TypeHandle(ITypeSymbol type)
        {
            if (type.IsArray(out var elementType))
            {
                var th = TypeHandle(elementType);
                return new ReflectionHandleModel { Value = th.Value | (ulong)TypeHandleFlags.Array };
            }
            var name = type.CreateFullTypeName(global, withGlobalNamespace:false);
            int typeHandle = Array.IndexOf(typeNames, name);
            if (typeHandle < 0)
                return new ReflectionHandleModel();
            return new ReflectionHandleModel { Value = (assemblyHandle << ReflectionHandleModel.AssemblyShift) | ((ulong)typeHandle << ReflectionHandleModel.TypeShift) };
        }

        ReflectionHandleModel MemberHandle(ISymbol type)
        {
            var typeHandle = TypeHandle(type.ContainingType);
            var index = type.ContainingType.GetMembers().IndexOf(type);
            return typeHandle with { Value = typeHandle.Value | ((ulong)index << ReflectionHandleModel.MemberShift) };
        }

        KnownTypeHandle KnownTypeFromName(string t)
        {
            return t == "System.Object" ? KnownTypeHandle.SystemObject :
                        t == "System.Boolean" ? KnownTypeHandle.SystemBool :
                        t == "System.Char" ? KnownTypeHandle.SystemChar :
                        t == "System.SByte" ? KnownTypeHandle.SystemSByte :
                        t == "System.Byte" ? KnownTypeHandle.SystemByte :
                        t == "System.Int16" ? KnownTypeHandle.SystemInt16 :
                        t == "System.UInt16" ? KnownTypeHandle.SystemUInt16 :
                        t == "System.Int32" ? KnownTypeHandle.SystemInt32 :
                        t == "System.UInt32" ? KnownTypeHandle.SystemUint32 :
                        t == "System.Int64" ? KnownTypeHandle.SystemInt64 :
                        t == "System.UInt64" ? KnownTypeHandle.SystemUint64 :
                        t == "System.Float" ? KnownTypeHandle.SystemFloat :
                        t == "System.Single" ? KnownTypeHandle.SystemSingle :
                        t == "System.Double" ? KnownTypeHandle.SystemDouble :
                        t == "System.Array" ? KnownTypeHandle.SystemArray : KnownTypeHandle.Unknown;
        }

        public AssemblyModel FromAssemblySymbol(IAssemblySymbol assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            if (!global.HasAttribute(assembly, typeof(AssemblyHandleAttribute).FullName, null, false, out var args))
            {
                //throw new InvalidOperationException("An AssemblyHandleAttribute must be defined on all assembly");
            }
            assemblyHandle = (uint)(args?[0] ?? (uint)new Random().Next(32768, 65536));
            //uint assemblyHandle = (uint)args[0];
            var types = assembly.GlobalNamespace
                    .GetNamespaceMembers()
                    .SelectMany(GetAllTypes);
            typeNames = new string[] { "" }.Concat(types.Select(t => t.CreateFullTypeName(global, withGlobalNamespace:false))).ToArray();
            //make sure unknown type is index zero, System.Object is at index 1
            typeNames = typeNames.OrderBy(t =>
            t == "" ? (int)KnownTypeHandle.Unknown :
            t == "System.Object" ? (int)KnownTypeHandle.SystemObject :
            t == "System.Boolean" ? (int)KnownTypeHandle.SystemBool :
            t == "System.Char" ? (int)KnownTypeHandle.SystemChar :
            t == "System.SByte" ? (int)KnownTypeHandle.SystemSByte :
            t == "System.Byte" ? (int)KnownTypeHandle.SystemByte :
            t == "System.Int16" ? (int)KnownTypeHandle.SystemInt16 :
            t == "System.UInt16" ? (int)KnownTypeHandle.SystemUInt16 :
            t == "System.Int32" ? (int)KnownTypeHandle.SystemInt32 :
            t == "System.UInt32" ? (int)KnownTypeHandle.SystemUint32 :
            t == "System.Int64" ? (int)KnownTypeHandle.SystemInt64 :
            t == "System.UInt64" ? (int)KnownTypeHandle.SystemUint64 :
            t == "System.Float" ? (int)KnownTypeHandle.SystemFloat :
            t == "System.Single" ? (int)KnownTypeHandle.SystemSingle :
            t == "System.Double" ? (int)KnownTypeHandle.SystemDouble :
            t == "System.Array" ? (int)KnownTypeHandle.SystemArray : int.MaxValue).ToArray();
            var model = new AssemblyModel
            {
                AssemblyFlags = global.MainEntry != null ? AssemblyFlags.Entry : AssemblyFlags.None,
                Handle = new ReflectionHandleModel { Value = assemblyHandle },
                FullName = assembly.Identity.Name,
                Version = assembly.Identity.Version?.ToString() ?? "0.0.0.0",
                TypeNames = typeNames,
                Types = new ITypeSymbol?[] { null }.Concat(types)
                    .Select(FromTypeSymbol)
                    .ToArray(),
                Attributes = NullIfEmpty(assembly.GetAttributes()
                    .Where(a => a.AttributeClass != null)
                    .Where(a => global.ShouldExportType(a.AttributeClass!, null))
                    .Select(s => FromAttribute(s))
                    .ToArray())
            }; ;
            List<AssemblyManifestModel> manifests = new List<AssemblyManifestModel>();
            if (resxFiles != null)
            {
                foreach (var resx in resxFiles)
                {
                    var manifest = new AssemblyManifestModel();
                    manifest.Name = Path.GetFileNameWithoutExtension(resx);
                    var xml = File.ReadAllText(resx);
                    var doc = XElement.Parse(xml);
                    var result = doc.Elements("data").Where(r => r.Attribute("name") is not null && r.Element("value") is not null).ToDictionary(e => e.Attribute("name").Value, e => e.Element("value").Value);
                    manifest.StringResourceData = result;
                    manifests.Add(manifest);
                }
            }
            model.Manifests = manifests.ToArray();
            return model;
        }

        public TypeModel FromTypeSymbol(ITypeSymbol? symbol)
        {
            if (symbol == null) return new TypeModel { };
            var model = new TypeModel
            {
                //Name = symbol.Name,
                Handle = TypeHandle(symbol),
                //AssemblyQualifiedName = $"{RemoveGlobal(symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))}, {symbol.ContainingAssembly?.Name}",
                BaseType = symbol.BaseType != null ? TypeHandle(symbol.BaseType) : null,
                DeclaringType = symbol.ContainingType != null ? TypeHandle(symbol.ContainingType) : null,
                UnderlyingType = (symbol is INamedTypeSymbol nt && nt.EnumUnderlyingType != null)
                    ? TypeHandle(nt.EnumUnderlyingType)
                    : null,
                Kind = MapTypeKind(symbol.TypeKind),
                Flags = GetTypeFlagsModel(symbol),
                //TypeAttributes = 0,
                KnownType = KnownTypeFromName(symbol.CreateFullTypeName(global, withGlobalNamespace: false)),
                Properties = NullIfEmpty(symbol.GetMembers()
                            .Where(m => !m.IsExtern/*Extern methods are called via templates, not reflectable*/)
                            .Where(m => !m.DeclaredAccessibility.HasFlag(Accessibility.Internal)/*Internal methods are used by compiler only*/)
                            .OfType<IPropertySymbol>().Select(FromPropertySymbol).ToArray()),
                Methods = NullIfEmpty(symbol.GetMembers()
                            .Where(m => !m.IsExtern/*Extern methods are called via templates, not reflectable*/)
                            .Where(m => !m.DeclaredAccessibility.HasFlag(Accessibility.Internal)/*Internal methods are used by compiler only*/)
                            .OfType<IMethodSymbol>()
                            .Where(m => m.MethodKind == MethodKind.Ordinary)
                            .Where(m => !global.LinkTrimOutMethod(m))
                            .Select(FromMethodSymbol)
                            .ToArray()),
                Constructors = NullIfEmpty(symbol.GetMembers()
                            .Where(m => !m.IsExtern/*Extern methods are called via templates, not reflectable*/)
                            .Where(m => !m.DeclaredAccessibility.HasFlag(Accessibility.Internal)/*Internal methods are used by compiler only*/)
                            .OfType<IMethodSymbol>()
                            .Where(m => m.MethodKind == MethodKind.Constructor)
                            .Where(m => !global.LinkTrimOutMethod(m))
                            .Select(FromConstructorSymbol).ToArray()),
                Fields = NullIfEmpty(symbol.GetMembers()
                            .Where(m => !m.IsExtern/*Extern methods are called via templates, not reflectable*/)
                            .Where(m => !m.DeclaredAccessibility.HasFlag(Accessibility.Internal)/*Internal methods are used by compiler only*/)
                            .Where(m => !m.Name.Contains("k__BackingField")/*Property backing fields are not needed*/)
                            .OfType<IFieldSymbol>().Select(FromFieldSymbol).ToArray()),
                Events = NullIfEmpty(symbol.GetMembers()
                             .Where(m => !m.IsExtern/*Extern methods are called via templates, not reflectable*/)
                            .Where(m => !m.DeclaredAccessibility.HasFlag(Accessibility.Internal)/*Internal methods are used by compiler only*/)
                            .OfType<IEventSymbol>().Select(FromEventSymbol).ToArray()),
                Interfaces = NullIfEmpty(symbol.AllInterfaces.Where(i => global.ShouldExportType(i, null)).Select(i => TypeHandle(i)).ToArray()),
                Attributes = NullIfEmpty(symbol.GetAttributes()
                .Where(a => a.AttributeClass != null && global.ShouldExportType(a.AttributeClass, null))
                .Select(a => FromAttribute(a)).ToArray()),
                GenericArguments = NullIfEmpty(symbol is INamedTypeSymbol g && g.TypeArguments.Any()
                    ? g.TypeArguments.Select(t => TypeHandle(t)).ToArray()
                    : Array.Empty<ReflectionHandleModel>()),
                GenericConstraints = NullIfEmpty(Array.Empty<GenericParameterConstraintModel>()),
                NestedTypes = NullIfEmpty(symbol.GetTypeMembers().Where(t => !global.ShouldExportType(t, null)).Select(t => TypeHandle(t)).ToArray()),
                GenericParameterCount = symbol is INamedTypeSymbol ng ? ng.TypeParameters.Length : 0
            };

            return model;
        }

        // --- Internal helpers ---
        private IEnumerable<INamedTypeSymbol> GetInnerTypes(ITypeSymbol ns)
        {
            foreach (var nested in ns.GetTypeMembers())
            {
                yield return nested;
                foreach (var inner in GetInnerTypes(nested))
                    if (global.ShouldExportType(inner, null) && global.IsReflectable(inner, null))
                        yield return inner;
            }
        }
        private IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
        {
            foreach (var type in ns.GetTypeMembers())
            {
                if (global.ShouldExportType(type, null) && global.IsReflectable(type, null))
                    yield return type;
                foreach (var inner in GetInnerTypes(type))
                    yield return inner;
            }
            foreach (var nested in ns.GetNamespaceMembers())
            {
                foreach (var inner in GetAllTypes(nested))
                    if (global.ShouldExportType(inner, null) && global.IsReflectable(inner, null))
                        yield return inner;
            }
        }

        private static TypeKindModel MapTypeKind(Microsoft.CodeAnalysis.TypeKind roslynKind) =>
            roslynKind switch
            {
                Microsoft.CodeAnalysis.TypeKind.Class => TypeKindModel.Class,
                Microsoft.CodeAnalysis.TypeKind.Struct => TypeKindModel.Struct,
                Microsoft.CodeAnalysis.TypeKind.Interface => TypeKindModel.Interface,
                Microsoft.CodeAnalysis.TypeKind.Enum => TypeKindModel.Enum,
                Microsoft.CodeAnalysis.TypeKind.Delegate => TypeKindModel.Delegate,
                Microsoft.CodeAnalysis.TypeKind.Array => TypeKindModel.Array,
                Microsoft.CodeAnalysis.TypeKind.Pointer => TypeKindModel.Pointer,
                _ => TypeKindModel.Unknown
            };
        //private static TypeAttributes GetTypeAttributes(ITypeSymbol type)
        //{
        //    var flags = (CoreLibTypeAttributes)0;

        //    if (type.DeclaredAccessibility == Accessibility.Public)
        //        flags |= CoreLibTypeAttributes.Public;
        //    if (type.IsAbstract)
        //        flags |= CoreLibTypeAttributes.Abstract;
        //    if (type.IsSealed)
        //        flags |= CoreLibTypeAttributes.Sealed;
        //    if (type.TypeKind == Microsoft.CodeAnalysis.TypeKind.Interface)
        //        flags |= CoreLibTypeAttributes.Interface;
        //    if (type.TypeKind == Microsoft.CodeAnalysis.TypeKind.Enum)
        //        flags |= CoreLibTypeAttributes.Enum;
        //    if (type.IsValueType)
        //        flags |= TypeFlagsModel.IsValueType;
        //    if (type is INamedTypeSymbol named && named.IsGenericType)
        //        flags |= TypeFlagsModel.IsGenericType;
        //    if (type.TypeKind == Microsoft.CodeAnalysis.TypeKind.Class)
        //        flags |= TypeFlagsModel.IsClass;
        //    if (type.BaseType?.Name == "Enum" && type.TypeKind == Microsoft.CodeAnalysis.TypeKind.Enum)
        //        flags |= TypeFlagsModel.IsFlags;
        //    if (type.TypeKind == Microsoft.CodeAnalysis.TypeKind.Array)
        //        flags |= TypeFlagsModel.IsArray;
        //    if (type.ContainingSymbol.Kind == Microsoft.CodeAnalysis.SymbolKind.NamedType)
        //        flags |= TypeFlagsModel.IsNested;

        //    return flags;
        //}


        private static TypeFlagsModel GetTypeFlagsModel(ITypeSymbol type)
        {
            var flags = TypeFlagsModel.None;

            if (type.DeclaredAccessibility == Accessibility.Public)
                flags |= TypeFlagsModel.IsPublic;
            if (type.IsAbstract)
                flags |= TypeFlagsModel.IsAbstract;
            if (type.IsSealed)
                flags |= TypeFlagsModel.IsSealed;
            if (type.TypeKind == Microsoft.CodeAnalysis.TypeKind.Interface)
                flags |= TypeFlagsModel.IsInterface;
            if (type.TypeKind == Microsoft.CodeAnalysis.TypeKind.Enum)
                flags |= TypeFlagsModel.IsEnum;
            if (type.IsValueType)
                flags |= TypeFlagsModel.IsValueType;
            if (type is INamedTypeSymbol named && named.IsGenericType)
                flags |= TypeFlagsModel.IsGenericType;
            if (type.TypeKind == Microsoft.CodeAnalysis.TypeKind.Class)
                flags |= TypeFlagsModel.IsClass;
            if (type.BaseType?.Name == "Enum" && type.TypeKind == Microsoft.CodeAnalysis.TypeKind.Enum)
                flags |= TypeFlagsModel.IsFlags;
            if (type.TypeKind == Microsoft.CodeAnalysis.TypeKind.Array)
                flags |= TypeFlagsModel.IsArray;
            if (type.ContainingSymbol.Kind == Microsoft.CodeAnalysis.SymbolKind.NamedType)
                flags |= TypeFlagsModel.IsNested;

            return flags;
        }

        PropertyModel FromPropertySymbol(IPropertySymbol prop)
        {
            var name = prop.Name;
            //a method that implements explicitly will have a long name qualified by the interface it is defined on
            //Let's shrink it by removing the interface name
            if (prop.ExplicitInterfaceImplementations.Any())
            {
                var ex = prop.ExplicitInterfaceImplementations.First().ContainingType;
                var handle = TypeHandle(ex);
                name = (handle.Value > 0 ? $"{{{handle.Value}}}." : "") + name.Split('.').Last();
            }
            var metadata = global.GetRequiredMetadata(prop);
            var outputName = metadata.OverloadName ?? name;
            return new PropertyModel
            {
                Name = name,
                OutputName = outputName != name ? outputName : null,
                DeclaringType = TypeHandle(prop.ContainingType),
                Flags = GetMemberFlagsModel(prop),
                PropertyType = !global.ShouldExportType(prop.Type, null) ? default : TypeHandle(prop.Type),
                IndexParameters = prop.Parameters.Select(FromParameterSymbol).ToArray(),
                GetMethod = prop.GetMethod != null ? FromMethodSymbol(prop.GetMethod) : null,
                SetMethod = prop.SetMethod != null ? FromMethodSymbol(prop.SetMethod) : null,
                Handle = MemberHandle(prop),
                Attributes = NullIfEmpty(prop.GetAttributes()
                .Where(a => a.AttributeClass != null && global.ShouldExportType(a.AttributeClass, null))
                .Select(a => FromAttribute(a)).ToArray())
            };
        }

        static string? NullIfVoid(string t)
        {
            if (t == "void")
                return null;
            return t;
        }

        MethodModel FromMethodSymbol(IMethodSymbol method)
        {
            var name = method.Name;
            //a method that implements explicitly will have a long name qualified by the interface it is defined on
            //Let's shrink it by removing the interface name
            if (method.ExplicitInterfaceImplementations.Any())
            {
                var ex = method.ExplicitInterfaceImplementations.First().ContainingType;
                var handle = TypeHandle(ex);
                name = (handle.Value > 0 ? $"{{{handle.Value}}}." : "") + name.Split('.').Last();
            }
            var metadata = global.GetRequiredMetadata(method);
            var outputName = metadata.OverloadName ?? name;
            return new MethodModel
            {
                Name = name,
                OutputName = outputName != name ? outputName : null,
                DeclaringType = TypeHandle(method.ContainingType),
                Flags = GetMemberFlagsModel(method),
                ReturnType = !global.ShouldExportType(method.ReturnType, null) ? default : TypeHandle(method.ReturnType),
                Parameters = NullIfEmpty(method.Parameters.Select(FromParameterSymbol).ToArray()),
                GenericArguments = NullIfEmpty(method.TypeArguments.Select(t => !global.ShouldExportType(t, null) ? "object" : t.CreateFullTypeName(global, withGlobalNamespace: false)).ToArray()),
                Handle = MemberHandle(method),
                Attributes = NullIfEmpty(method.GetAttributes()
                .Where(a => a.AttributeClass != null && global.ShouldExportType(a.AttributeClass, null))
                .Select(a => FromAttribute(a)).ToArray())
            };
        }

        ConstructorModel FromConstructorSymbol(IMethodSymbol ctor)
        {
            var metadata = global.GetRequiredMetadata(ctor);
            var outputName = metadata.OverloadName ?? ctor.Name;
            return new ConstructorModel
            {
                Name = ctor.Name,
                OutputName = outputName != ctor.Name ? outputName : null,
                DeclaringType = TypeHandle(ctor.ContainingType),
                Flags = GetMemberFlagsModel(ctor),
                Parameters = NullIfEmpty(ctor.Parameters.Select(FromParameterSymbol).ToArray()),
                Handle = MemberHandle(ctor),
                Attributes = NullIfEmpty(ctor.GetAttributes()
                .Where(a => a.AttributeClass != null && global.ShouldExportType(a.AttributeClass, null))
                .Select(a => FromAttribute(a)).ToArray())
            };
        }

        FieldModel FromFieldSymbol(IFieldSymbol field)
        {
            var metadata = global.GetRequiredMetadata(field);
            var outputName = metadata.OverloadName ?? field.Name;
            return new FieldModel
            {
                Name = field.Name,
                OutputName = outputName != field.Name ? outputName : null,
                DeclaringType = !global.ShouldExportType(field.Type, null) ? default : TypeHandle(field.ContainingType),
                Flags = GetMemberFlagsModel(field),
                FieldType = !global.ShouldExportType(field.Type, null) ? default : TypeHandle(field.Type),
                Handle = MemberHandle(field),
                Attributes = NullIfEmpty(field.GetAttributes()
                .Where(a => a.AttributeClass != null && global.ShouldExportType(a.AttributeClass, null))
                .Select(a => FromAttribute(a)).ToArray())
            };
        }

        EventModel FromEventSymbol(IEventSymbol ev)
        {
            var metadata = global.GetRequiredMetadata(ev);
            var outputName = metadata.OverloadName ?? ev.Name;
            return new EventModel
            {
                Name = ev.Name,
                OutputName = outputName != ev.Name ? outputName : null,
                DeclaringType = TypeHandle(ev.ContainingType),
                Flags = GetMemberFlagsModel(ev),
                EventHandlerType = !global.ShouldExportType(ev.Type, null) ? default : TypeHandle(ev.Type),
                AddMethod = ev.AddMethod != null ? FromMethodSymbol(ev.AddMethod) : null,
                RemoveMethod = ev.RemoveMethod != null ? FromMethodSymbol(ev.RemoveMethod) : null,
                RaiseMethod = ev.RaiseMethod != null ? FromMethodSymbol(ev.RaiseMethod) : null,
                Handle = MemberHandle(ev),
                Attributes = NullIfEmpty(ev.GetAttributes()
                .Where(a => a.AttributeClass != null && global.ShouldExportType(a.AttributeClass, null))
                .Select(a => FromAttribute(a)).ToArray())
            };
        }

        ParameterModel FromParameterSymbol(IParameterSymbol param) => new ParameterModel
        {
            Name = param.Name,
            ParameterType = !global.ShouldExportType(param.Type, null) ? default : TypeHandle(param.Type),
            Position = param.Ordinal,
            Flags =
                (param.IsOptional ? ParameterFlagsModel.Optional : ParameterFlagsModel.None) |
                (param.RefKind == RefKind.Out ? ParameterFlagsModel.Out : ParameterFlagsModel.None) |
                (param.RefKind == RefKind.Ref ? ParameterFlagsModel.Ref : ParameterFlagsModel.None) |
                (param.IsParams ? ParameterFlagsModel.Params : ParameterFlagsModel.None),
            DefaultValue = param.HasExplicitDefaultValue ? param.ExplicitDefaultValue ?? "__typeDefault__" : null,
            Attributes = NullIfEmpty(param.GetAttributes()
                .Where(a => a.AttributeClass != null && global.ShouldExportType(a.AttributeClass, null))
                .Select(a => FromAttribute(a)).ToArray())
        };

        AttributeModel FromAttribute(AttributeData att)
        {
            return new AttributeModel
            {
                TypeHandle = att.AttributeClass == null ? default : TypeHandle(att.AttributeClass),
                ConstructorHandle = att.AttributeConstructor == null ? default : MemberHandle(att.AttributeConstructor),
                ConstructorArguments = att.ConstructorArguments.Select(arg => new AttributeConstructorArgumentModel
                {
                    Type = arg.Type != null ? TypeHandle(arg.Type) : default,
                    //Value = arg.Kind == TypedConstantKind.Array ? arg.Values : arg.Value,
                }).ToArray(),
                NamedArguments = att.NamedArguments.Select(arg => new AttributeNamedArgumentModel
                {
                    Name = arg.Key,
                    Type = arg.Value.Type != null ? TypeHandle(arg.Value.Type) : default,
                    //Value = arg.Value.Kind == TypedConstantKind.Array ? arg.Value.Values : arg.Value.Value,
                }).ToArray()
            };
        }

        private static MemberFlagsModel GetMemberFlagsModel(ISymbol symbol)
        {
            var flags = MemberFlagsModel.None;

            switch (symbol.DeclaredAccessibility)
            {
                case Accessibility.Public: flags |= MemberFlagsModel.IsPublic; break;
                case Accessibility.Private: flags |= MemberFlagsModel.IsPrivate; break;
                case Accessibility.Protected: flags |= MemberFlagsModel.IsFamily; break;
                case Accessibility.Internal: flags |= MemberFlagsModel.IsAssembly; break;
                case Accessibility.ProtectedOrInternal: flags |= MemberFlagsModel.IsFamilyOrAssembly; break;
            }

            if (symbol.IsStatic) flags |= MemberFlagsModel.IsStatic;
            if (symbol.IsAbstract) flags |= MemberFlagsModel.IsAbstract;
            if (symbol.IsVirtual) flags |= MemberFlagsModel.IsVirtual;
            if (symbol.IsOverride) flags |= MemberFlagsModel.IsOverride;
            if (symbol is IMethodSymbol m && m.IsAsync) flags |= MemberFlagsModel.IsAsync;
            if (symbol.IsSealed) flags |= MemberFlagsModel.IsSealed;

            return flags;
        }
    }
}
