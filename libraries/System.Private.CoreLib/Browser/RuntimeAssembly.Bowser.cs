using dotnetJs;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Reflection
{
    [dotnetJs.ForcePartial(typeof(RuntimeAssembly))]
    [dotnetJs.Name(nameof(RuntimeAssembly))]
    [dotnetJs.Boot]
    [dotnetJs.Reflectable(false)]
    [dotnetJs.OutputOrder(int.MinValue + 1)] //make sure we emit this type immediately after AppDomain
    internal sealed partial class RuntimeAssembly_Partial : ForcedPartialBase<RuntimeAssembly>
    {
        internal RuntimeModule_Partial _module;
        internal RuntimeType[] _types = [];
        internal AssemblyModel _model;
        public RuntimeAssembly_Partial(AssemblyModel model, string assemblyName)
        {
            this._model = model;
            _module = new RuntimeModule_Partial(this);
            if (model.AssemblyFlags.TypeHasFlag(AssemblyFlags.Entry))
                Assembly._entry = this.As<Assembly>();
        }

        /// <summary>
        /// Define a proxy to a type/prototype not yet created. 
        /// </summary>
        /// <param name="fullTypeName"></param>
        [dotnetJs.Name(dotnetJs.Constants.AssemblyTypeProxyName)]
        public void TypeProxy(string fullTypeName)
        {
            if (!AppDomain.GlobalPrototypeRegistry.ContainsKey(fullTypeName))
            {
                var proxyHandler = new TypeProxyHandler(fullTypeName);
                object? proxy = null;
                Script.Write("proxy = new Proxy({}, proxyHandler)");
                //var type = Type.Create(this, (TypePrototype?)null, null!, fullTypeName); //will be initialized when finally created
                //_stubs[fullTypeName] = type;
                AppDomain.GlobalPrototypeRegistry.SetNested(fullTypeName.NativeReplaceAll("<", "$").NativeReplaceAll(",", "$").NativeReplaceAll(">", "$"), proxy.As<TypePrototype>());
            }
        }

        [dotnetJs.Name(dotnetJs.Constants.AssemblyClassName)]
        public Union<TypePrototype, TypePrototypeProvider> DefineType(string fullTypeName, TypePrototypeProvider provider)
        {
            provider.As<object>()["$fn"] = fullTypeName;
            var jsName = fullTypeName.NativeReplaceAll("<", "$").NativeReplaceAll(",", "$").NativeReplaceAll(">", "$");
            TypeModel? typeMetadata;
            unchecked
            {
                typeMetadata = _model.Types?.Filter(t => _model.TypeNames[t.Handle.Type] == fullTypeName)[0];
            }
            if (Script.IsUndefinedOrNull(typeMetadata))
            {
                //var pth = prototype.FullName.Split('.');
                //var pth = prototype.FullName.NativeSplit(".");
                typeMetadata = new TypeModel
                {
                    //Name = pth[pth.Length - 1],
                    Handle = new ReflectionHandleModel { Value = 0 },//prototype.FullName,
                };
            }
            //Dont try reading namespace for nested types, it will return the nested static method/property within the containing class anyway
            var existing = !typeMetadata!.Flags.TypeHasFlag(TypeFlagsModel.IsNested) ? AppDomain.GlobalPrototypeRegistry.GetNested(jsName) : null;
            if (Script.IsDefined(existing))
            {
                //if we have created a typestub, this is existing as Proxy type with handler TypeProxyHandler, now we have its prototype
#pragma warning disable CS0184 // 'is' expression's given expression is never of the provided type
                //if (!(existing is TypeProxyHandler))
                if (!Script.Write<bool>("existing instanceof Proxy"))
                    return existing!;
#pragma warning restore CS0184 // 'is' expression's given expression is never of the provided type
            }
            //bool isGenericDefinition = fullTypeName.EndsWith('$');
            bool isGenericDefinition = fullTypeName.NativeEndsWith("$") || fullTypeName.NativeEndsWith(">");
            bool isInterface = typeMetadata!.Kind == TypeKindModel.Interface;
            bool isInterfaceMixin = isInterface && Script.Write<int>("provider.length") >= 1;
            //check if we have a proxy for this type already
            //Type? type = existing.As<Type>();
            RuntimeType? type = null;
            //if (Script.IsDefined(type))
            //{
            //    Script.Delete(_stubs, fullTypeName);
            //}
            TypePrototype? prototype = null;
            if (isInterfaceMixin)
            {
                //if (Script.IsDefined(type))
                //{
                //    type.InitializeFrom(provider, typeMetadata!);
                //}
                //else
                //{
                type = RuntimeType.Create(This, provider, typeMetadata!, fullTypeName);
                //}
            }
            else if (isGenericDefinition)
            {
                //if (Script.IsDefined(type))
                //{
                //    type.InitializeFrom(provider, typeMetadata!);
                //}
                //else
                //{
                type = RuntimeType.Create(This, provider, typeMetadata!, fullTypeName);
                //}
            }
            else
            {
                prototype = provider(null, null);
                //if (Script.IsDefined(type))
                //{
                //    type.InitializeFrom(prototype, typeMetadata!);
                //}
                //else
                //{
                type = RuntimeType.Create(This, prototype, typeMetadata!, fullTypeName);
                //}
                prototype.Type = type;
            }
            _types.Push(type);
            AppDomain.GlobalTypeRegistry[fullTypeName] = type;
            AppDomain.GlobalTypeRegistry[typeMetadata.Handle.AssemblyAndType] = type;
            if (Script.IsDefined(existing))
            {
                //Now that we have the concrete type and some closure already holds the stub/proxy
                //Supply the real things to the proxy so it can forward it as neccessary
                existing.As<TypeProxyHandler>().TargetType = type;
                existing.As<TypeProxyHandler>().Prototype = prototype;
                //remove the typeStub just before we insert the real type
                AppDomain.GlobalPrototypeRegistry.RemoveNested(jsName);
            }
            //dont try so set inner types, they are managed and readonly static within the containing type
            if (!typeMetadata.Flags.TypeHasFlag(TypeFlagsModel.IsNested))
                AppDomain.GlobalPrototypeRegistry.SetNested(jsName, prototype ?? provider.As<TypePrototype>());
            if (!isInterfaceMixin && !isGenericDefinition)
                AppDomain.SetupDefaults(type);
            //if assembly was already built before we make this new type, build it immediately
            if (isCompleted)
                type.Complete();
            return prototype ?? provider.As<TypePrototype>();
        }

        //static SimpleDictionary<TypePrototype> mixinCache = new SimpleDictionary<TypePrototype>();
        [dotnetJs.Name("$mix")]
        TypePrototype Mixin(string fullTypeName, TypePrototype[] genericArgumemnts, TypePrototype? mix, ParameterlessTypePrototypeProvider getPrototype)
        {
            unchecked
            {
                string cacheKey;
                string? newGenericName = null;
                if (genericArgumemnts.Length > 0)
                {
                    if (!fullTypeName.NativeEndsWith(">"))
                        throw new InvalidOperationException();
                    int nArgs = 1;
                    int i = fullTypeName.Length - 2;
                    while (fullTypeName[i] != '<')
                    {
                        nArgs++;
                        i--;
                    }
                    var name = fullTypeName.NativeSplit("<")[0];
                    if (nArgs != genericArgumemnts.Length)
                        throw new InvalidOperationException("Numver of generic arguments doesnt match");
                    var gn = genericArgumemnts.Map(m => m?.FullName ?? "").Join(",");
                    newGenericName = name + "<" + gn + ">";
                    cacheKey = newGenericName;
                    if (Script.IsDefined(mix))
                    {
                        cacheKey += "+" + mix!.FullName;
                    }
                }
                else
                {
                    cacheKey = fullTypeName;
                    if (Script.IsDefined(mix))
                        cacheKey += "+" + mix!.FullName;
                }
                var existingPrototype = AppDomain.GlobalPrototypeRegistry[cacheKey];
                if (Script.IsDefined(existingPrototype))
                    return existingPrototype.As<TypePrototype>();
                var prototype = getPrototype();
                AppDomain.GlobalPrototypeRegistry[cacheKey] = prototype;
                //this is a new class prototype, define its System.Type if any of the typArgument is not a genericName
                if (genericArgumemnts.Length > 0 && genericArgumemnts.Some(t =>
                {
                    //It is very much possible that the t(TypePrototype) we have here is actually a System.Type, if we had created a stub of it that isn't replace yet
                    //But we can be very sure it isn't a generic type
#pragma warning disable CS0184 // 'is' expression's given expression is never of the provided type
                    if (t is Type)
                    {
                        //the only thing the stub has at this point is just its fullName
                        return !t.As<Type>().FullName!.NativeEndsWith(">");
                        //return false;
                    }
#pragma warning restore CS0184 // 'is' expression's given expression is never of the provided type
                    return !t.Type!.IsGenericTypeDefinition;
                }))
                {
                    var genericType = AppDomain.GlobalTypeRegistry[fullTypeName];
                    var newType = genericType.MakeGenericTypeInternal(genericArgumemnts.Map(a => a.Type.As<RuntimeType>()!), prototype);
                    AppDomain.GlobalTypeRegistry[newGenericName!] = newType;
                    //prototype.Type = newType;
                    newType.Complete();
                }
                return prototype;
            }
        }

        [dotnetJs.Name(Constants.InterfaceMixin)]
        public TypePrototype InterfaceMixin(string fullName, TypePrototype[] mixes, ParameterlessTypePrototypeProvider getPrototype)
        {
            if (mixes.Length != 1)
                throw new InvalidOperationException("Interface mixin must be 1");
            unchecked
            {
                return Mixin(fullName, [], mixes[0], getPrototype);
            }
        }

        [dotnetJs.Name(Constants.GenericInterfaceMixin)]
        public TypePrototype GenericInterfaceMixin(string fullName, TypePrototype[] mixes, ParameterlessTypePrototypeProvider getPrototype)
        {
            if (mixes.Length < 2)
                throw new InvalidOperationException("Generic Interface mixin must be at least 2");
            unchecked
            {
                return Mixin(fullName, mixes.Slice(0, mixes.Length - 1).As<TypePrototype[]>(), mixes[mixes.Length - 1], getPrototype);
            }
        }

        [dotnetJs.Name(dotnetJs.Constants.GenericType)]
        public TypePrototype GenericType(string fullName, TypePrototype[] genericArgs, ParameterlessTypePrototypeProvider getPrototype)
        {
            return Mixin(fullName, genericArgs, null, getPrototype);
        }

        bool isCompleted;
        internal void Complete()
        {
            unchecked
            {
                //there is a bit of dilemma here
                //Calling complete on Type runs the static initializers,
                //however if the  dependency of this is not yet run, we end up getting TypeError
                //For now, we run complete until successful
                RuntimeType[] remaining = [];
                remaining.Push(_types);
                while (remaining.Length > 0)
                {
                    int startLen = remaining.Length;
                    for (int i = 0; i < remaining.Length; i++)
                    {
                        try
                        {
                            remaining[i].Complete();
                            remaining.Splice(i, 1);
                            //move iterator back to where we removed one
                            i--;
                        }
                        catch
                        {
                            //Debugger.Break();
                        }
                    }
                    if (remaining.Length == startLen) //cannot initialize the remaining type
                    {
                        //try them one more time and don't catch the error again sowe can see it in console
                        for (int i = 0; i < remaining.Length; i++)
                        {
                            remaining[i].Complete();
                            remaining.Splice(i, 1);
                        }
                        break;
                    }
                }
            }
            isCompleted = true;
        }


        internal RuntimeType? GetTypeInternal(string name, bool ignoreCase = false)
        {
            for (int i = 0; i < _types.Length; i++)
            {
                var t = _types[i];
                if (t.InternalFullName == name || t.InternalAssemblyQualifiedName == name)
                    return t;
                if (ignoreCase)
                {
                    if (t.InternalFullName.Equals(name, StringComparison.InvariantCulture) || t.InternalAssemblyQualifiedName.Equals(name))
                        return t;
                }
            }
            return null;
        }


        [dotnetJs.MemberReplace]
        private static void GetEntryPoint(QCallAssembly assembly, ObjectHandleOnStack res)
        {
            var massembly = assembly.QCallAssemblyHandleToRuntimeType().As<RuntimeAssembly_Partial>();
            var model = massembly._model;
            var method = (MethodInfo?)AppDomain.GetMember(model.Entry);
            res.GetObjectHandleOnStack<MethodInfo?>() = method;
        }

        [dotnetJs.MemberReplace]
        private static void GetManifestResourceNames(QCallAssembly assembly_h, ObjectHandleOnStack res)
        {
            var assembly = assembly_h.QCallAssemblyHandleToRuntimeType().As<RuntimeAssembly_Partial>();
            var names = assembly._model.Manifests?.Map(e => e.Name);
            res.GetObjectHandleOnStack<string[]?>() = names;
        }

        [dotnetJs.MemberReplace]
        private static void GetExportedTypes(QCallAssembly assembly_h, ObjectHandleOnStack res)
        {
            var assembly = assembly_h.QCallAssemblyHandleToRuntimeType().As<RuntimeAssembly_Partial>();
            res.GetObjectHandleOnStack<Type[]?>() = assembly._types.Filter(e => e._model.Flags.TypeHasFlag(TypeFlagsModel.IsPublic));
        }

        [dotnetJs.MemberReplace]
        private static void GetTopLevelForwardedTypes(QCallAssembly assembly_h, ObjectHandleOnStack res)
        {
            var assembly = assembly_h.QCallAssemblyHandleToRuntimeType().As<RuntimeAssembly_Partial>();
            res.GetObjectHandleOnStack<Type[]?>() = [];
        }

        [dotnetJs.MemberReplace]
        private static void GetInfo(QCallAssembly assembly, ObjectHandleOnStack res, int kind)
        {
            var runtimeAssembly = assembly.QCallAssemblyHandleToRuntimeType().As<RuntimeAssembly_Partial>();
            switch (kind)
            {
                //Location
                case 1:
                    res.GetObjectHandleOnStack<string?>() = "localhost";
                    break;
                //CodeBase = 2,
                case 2:
                    res.GetObjectHandleOnStack<string?>() = "localhost";
                    break;
                //FullName = 3,
                case 3:
                    res.GetObjectHandleOnStack<string>() = runtimeAssembly._model.FullName;
                    break;
                //ImageRuntimeVersion = 4
                case 4:
                    res.GetObjectHandleOnStack<string>() = runtimeAssembly._model.Version;
                    break;
            }
        }

        [dotnetJs.MemberReplace]
        private static bool GetManifestResourceInfoInternal(QCallAssembly assembly, string name, ManifestResourceInfo info)
        {
            var runtimeAssembly = assembly.QCallAssemblyHandleToRuntimeType().As<RuntimeAssembly_Partial>();
            var manifest = runtimeAssembly._model.Manifests?.ArrayFirstOrDefault(a => a.Name == name);
            if (manifest != null)
            {
                //info.ResourceLocation = ResourceLocation.Embedded;
                dotnetJs.Script.Debugger();
                return true;
            }
            return false;
        }

        [dotnetJs.MemberReplace]
        private static IntPtr /* byte* */ GetManifestResourceInternal(QCallAssembly assembly, string name, out int size, ObjectHandleOnStack module)
        {
            var runtimeAssembly = assembly.QCallAssemblyHandleToRuntimeType().As<RuntimeAssembly_Partial>();
            var manifest = runtimeAssembly._model.Manifests?.ArrayFirstOrDefault(a => a.Name == name);
            if (manifest != null)
            {
                size = manifest.Data.Length;
                module.GetObjectHandleOnStack<RuntimeModule_Partial>() = runtimeAssembly._module;
                return RuntimeHelpers.CreateArrayReference(manifest.Data).As<IntPtr>();
            }
            size = 0;
            return IntPtr.Zero;
        }

        [dotnetJs.MemberReplace]
        private static void GetManifestModuleInternal(QCallAssembly assembly, ObjectHandleOnStack res)
        {
            var runtimeAssembly = assembly.QCallAssemblyHandleToRuntimeType().As<RuntimeAssembly_Partial>();
            res.GetObjectHandleOnStack<RuntimeModule_Partial>() = runtimeAssembly._module;
        }

        [dotnetJs.MemberReplace]
        private static void GetModulesInternal(QCallAssembly assembly, ObjectHandleOnStack res)
        {
            var runtimeAssembly = assembly.QCallAssemblyHandleToRuntimeType().As<RuntimeAssembly_Partial>();
            res.GetObjectHandleOnStack<RuntimeModule_Partial[]>() = [runtimeAssembly._module];
        }

        [dotnetJs.MemberReplace]
        private static extern IntPtr InternalGetReferencedAssemblies(Assembly assembly);

        [dotnetJs.MemberReplace(nameof(RuntimeAssembly.GetReferencedAssemblies))]
        internal static AssemblyName[] GetReferencedAssembliesOverride(Assembly assembly)
        {
            var runtimeAssembly = assembly.As<RuntimeAssembly_Partial>();
            return runtimeAssembly._model.ReferencedAssembliesHandle.Map(h => AppDomain.GetAssemblyName(h)).Filter(h => h != null).Map(n => new AssemblyName(n!));
        }

        [dotnetJs.MemberReplace]
        private static unsafe bool InternalTryGetRawMetadata(QCallAssembly assembly, out byte* blob, out int length)
        {
            throw new NotSupportedException();
        }

    }
}
