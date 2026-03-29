using NetJs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace System
{
    [NetJs.Boot]
    [NetJs.Reflectable(false)]
    [NetJs.OutputOrder(int.MinValue+2)] 
    public sealed partial class AppDomain
    {
        internal static SimpleDictionary<AssemblyModel> _reflectionMetadata = new SimpleDictionary<AssemblyModel>();
        internal static SimpleDictionary<RuntimeAssembly> _assemblies = new SimpleDictionary<RuntimeAssembly>();
        internal static SimpleDictionary<Union<TypePrototype, TypePrototypeProvider>> GlobalPrototypeRegistry;
        internal static SimpleDictionary<RuntimeType> GlobalTypeRegistry = new SimpleDictionary<RuntimeType>();
        
        static AppDomain()
        {
            //GlobalPrototypeRegistry = Script.Write<SimpleDictionary<TypePrototypeRegistrar>>("window.dotnetJs");
            GlobalPrototypeRegistry = Script.Write<SimpleDictionary<Union<TypePrototype, TypePrototypeProvider>>>("window.dotnetJs");
            Script.Write($"$.{Constants.AssemblyRegistryName} = this.{Constants.AssemblyRegistryName}");
            Script.Write($"$.{Constants.AssemblyMetadataRegistryName} = this.{Constants.AssemblyMetadataRegistryName}");
            //Script.Write($"$.{Constants.AssemblyStubName} = $.System.AppDomain.{Constants.AssemblyStubName}");
        }

        [Name(Constants.AssemblyMetadataRegistryName)]
        public static void ReflectionData(string assemblyName, AssemblyModel assemblyMetadata)
        {
            _reflectionMetadata[assemblyMetadata.Handle.Assembly] = assemblyMetadata;
            _reflectionMetadata[assemblyName] = assemblyMetadata;
        }

        [Name(Constants.AssemblyRegistryName)]
        internal static void CreateAssembly(string assemblyName, Action<RuntimeAssembly> action)
        {
            var assembly = _assemblies[assemblyName];
            if (Script.IsUndefinedOrNull(assembly))
            {
                var metadata = _reflectionMetadata[assemblyName];
                assembly = new RuntimeAssembly_Partial(metadata, assemblyName).As<RuntimeAssembly>();
                _assemblies[metadata.Handle.Assembly] = assembly;
                _assemblies[assemblyName] = assembly;
                //precreate all types in this assembly as a stub
                //if (Script.IsDefined(metadata.Types))
                //{
                //    for (int i = 0; i < metadata.TypeNames.Length;i++)
                //    {
                //        var name = metadata.TypeNames[i];
                //        var adjustedName = name.NativeReplace("<", "$").NativeReplace(",", "$").NativeReplace(">", "$");
                //        assembly.DefineStub(name);
                //    }
                //}
            }
            action(assembly);
            assembly.As<RuntimeAssembly_Partial>().Complete();
        }

        internal static void SetupDefaults(Type type)
        {
            //if (Script.TypeOf(type.Prototype) != "function")
            //{
            //    if (!Script.Write<bool>($"type._prototype.{Constants.IsTypeName}"))
            //    {
            //        bool Is(object value)
            //        {
            //            if (Script.InstanceOf(value, type))
            //                return true;
            //            return false;
            //        }
            //        type.Prototype[Constants.IsTypeName] = Is;
            //    }
            //}
        }

        //public Assembly[] GetAssemblies()
        //{
        //    return _assemblies.Values.Unique();
        //}

        //public static AppDomain CurrentDomain { get; } = new AppDomain();

        public static string GetTypeName(ReflectionHandleModel typeHandle)
        {
            var assemblyHandle = typeHandle.Assembly;
            var assemblyMetadata = _reflectionMetadata[assemblyHandle];
            return assemblyMetadata.TypeNames[typeHandle.Type];
        }

        public static string? GetAssemblyName(ReflectionHandleModel assemblyHandle)
        {
            var metadata = _reflectionMetadata[assemblyHandle.Assembly];
            return metadata?.FullName;
        }

        public static AssemblyModel? GetAssemblyMetadata(ReflectionHandleModel assemblyHandle)
        {
            var metadata = _reflectionMetadata[assemblyHandle.Assembly];
            return metadata;
        }

        internal static RuntimeAssembly? GetAssembly(ReflectionHandleModel assemblyHandle)
        {
            var assembly = _assemblies[assemblyHandle.Assembly];
            return assembly;
        }

        internal static RuntimeType? GetType(ReflectionHandleModel typeHandle)
        {
            var value = AppDomain.GlobalTypeRegistry[typeHandle.AssemblyAndType];
            if (Script.IsUndefined(value))
                return null;
            return value;
        }

        internal static MemberInfo? GetMember(ReflectionHandleModel memberHandle)
        {
            var type = GetType(memberHandle);
            return type.GetMemberInternal(memberHandle);
        }

        internal static RuntimeType? GetTypeInternal(string? typeName, bool ignoreCase = false, bool throwOnError = false)
        {
            if (typeName == null)
            {
                if (throwOnError)
                    throw new ArgumentNullException(nameof(typeName));
                return null;
            }
            var assemblies = CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                var type = assembly.As<RuntimeAssembly_Partial>().GetTypeInternal(typeName);
                if (type != null)
                    return type;
            }
            if (throwOnError)
                throw new InvalidOperationException($"Cannot find {typeName}");
            return null;
        }

    }
}
