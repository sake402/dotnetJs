using NetJs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;


namespace System
{
    [NetJs.Boot]
    [NetJs.Reflectable(false)]
    internal unsafe partial class RuntimeType
    {
        internal TypeModel _model;
        internal string _scriptFullName;
        internal RuntimeAssembly? _assembly;
        internal TypePrototypeProvider? _prototypeProvider;
        internal TypePrototype? _prototype;
        //TypeModel _metadata;
        internal RuntimeType? _parentGenericTypeDefinition = null;

        //for generic types
        internal RuntimeType[]? _typeArguments;

        //for type parameters
        internal int _genericParameterPosition = -1;
        internal GenericParameterConstraintModel? _constraintModel;
        internal RuntimeType[]? _typeConstraints;

        //for array types
        internal RuntimeType? _arrayElementType;
        internal int _arrayTypeRank;

        MethodInfo[] _methods = [];
        PropertyInfo[] _properties = [];
        FieldInfo[] _fields = [];
        ConstructorInfo[] _constructors = [];
        EventInfo[] _events = [];

        //we want a System.Type to extend its Javascript prototype so that we can do things like Type.StaticMember on both the System.Type and its equivalent JS Prototype.StaticMember
        //We however dont want the constructor of the js prototype called when we construct the mix
        internal static RuntimeType Create(RuntimeAssembly? assembly, Union<TypePrototypeProvider?, TypePrototype?>? prototype, TypeModel model, string scriptFullName)
        {
            //bool isClass = prototype != null && Script.Write<bool>("typeof prototype === 'function' && prototype.hasOwnProperty('prototype') && !prototype.hasOwnProperty('arguments')");
            //if (isClass)
            //{
            //    Script.Write($"const typeInfo = class extends $.$mix($.System.Type, prototype) {{ constructor(){{ }} }}");
            //    Script.Write($"const type = new typeInfo().__ctor__(assembly, prototype, model, scriptFullName)");
            //    Script.Write($"return type");
            //    return null!; //make compiler happy, we already returned
            //}
            //else
            {
                return new RuntimeType(assembly, prototype, model, scriptFullName);
            }
        }

        //DO NOT CALL DIRECTLY. We only call this in the static function above
        //we are merging these two constructor overload into one (with union) so there is no overload on System.Type and we can call it deterministically from Script.Write above
        [Name("__ctor__")] //we are renaming this constructor so it doesn't conflict with the js prototype we mix with it above
        private RuntimeType(RuntimeAssembly? assembly, Union<TypePrototypeProvider?, TypePrototype?>? prototype, TypeModel metadata, string scriptFullName)
        {
            _impl = new RuntimeTypeHandle((IntPtr)metadata.Handle.Value);
            bool isClass = prototype != null && Script.Write<bool>("typeof prototype === 'function' && prototype.hasOwnProperty('prototype') && !prototype.hasOwnProperty('arguments')");
            if (isClass)
            {
                _assembly = assembly;
                _prototype = prototype.As<TypePrototype>();
                _model = metadata;
                _scriptFullName = scriptFullName;
                if (metadata != null)
                    Initialize();
            }
            else
            {
                _assembly = assembly;
                _prototypeProvider = prototype.As<TypePrototypeProvider>();
                //if (prototypeProvider != null)
                //_prototype = prototypeProvider(null, null);
                _model = metadata;
                _scriptFullName = scriptFullName;
                if (metadata != null && _prototype != null)
                    Initialize();
            }
        }


        public string InternalAssemblyQualifiedName => InternalName + ", " + _assembly?.FullName;

        public string InternalName
        {
            get
            {
                if (InternalFullName == null)
                    return null;
                var split = InternalFullName.NativeSplit(",");
                return split[split.Length - 1];
            }
        }

        public string InternalFullName
        {
            get
            {
                return _prototype?.FullName ?? _scriptFullName;
                //var fullName = GetTypeNameFromHandle(_metadata.FullName);
                //if (IsGenericType)
                //{
                //    return fullName + "<" + string.Join(", ", GetGenericArguments().Map(t => t.FullName!)) + ">";
                //}
                //return fullName;
            }
        }
        public string InternalNamespace => InternalFullName.Substring(0, InternalFullName.LastIndexOf('.'));//?? null;

        //internal Type(Assembly assembly, TypePrototypeProvider? prototypeProvider, TypeModel metadata, string scriptFullName)
        //{
        //    _assembly = assembly;
        //    _prototypeProvider = prototypeProvider;
        //    //if (prototypeProvider != null)
        //    //_prototype = prototypeProvider(null, null);
        //    _metadata = metadata;
        //    _scriptFullName = scriptFullName;
        //    if (metadata != null && _prototype != null)
        //        Initialize();
        //}

        internal void InitializeFrom(TypePrototype? mprototype, TypeModel model)
        {
            _prototype = mprototype;
            _model = model;
            Initialize();
        }

        internal void InitializeFrom(TypePrototypeProvider? prototypeProvider, TypeModel model)
        {
            _prototypeProvider = prototypeProvider;
            //if (prototypeProvider != null)
            //_prototype = prototypeProvider(null, null);
            _model = model;
            Initialize();
        }

        void Initialize()
        {
            if (Script.IsDefined(_model.Methods))
            {
                _model.Methods!.ForEach(m =>
                {
                    var methodInfo = new RuntimeMethodInfo(m);
                    _methods.Push(methodInfo);
                });
            }
            if (Script.IsDefined(_model.Properties))
            {
                _model.Properties!.ForEach(m =>
                {
                    var propertyInfo = new RuntimePropertyInfo_Partial(m);
                    _properties.Push(propertyInfo.As<RuntimePropertyInfo>());
                });
            }
            if (Script.IsDefined(_model.Fields))
            {
                _model.Fields!.ForEach(m =>
                {
                    var fieldInfo = new RuntimeFieldInfo_Partial(m);
                    _fields.Push(fieldInfo.As<RuntimeFieldInfo>());
                });
            }
            if (Script.IsDefined(_model.Constructors))
            {
                _model.Constructors!.ForEach(m =>
                {
                    var constructorInfo = new RuntimeConstructorInfo(m);
                    _constructors.Push(constructorInfo);
                });
            }
            if (Script.IsDefined(_model.Events))
            {
                _model.Events!.ForEach(m =>
                {
                    var eventInfo = new RuntimeEventInfo_Partial(m);
                    _events.Push(eventInfo.As<RuntimeEventInfo>());
                });
            }
            if (_assembly != null && _prototype != null)
            {
                //bool isGenericDefinition = _scriptFullName.NativeEndsWith("$") || _scriptFullName.NativeEndsWith(">");
                //bool isInterface = _metadata!.Kind == TypeKindModel.Interface;
                //bool isInterfaceMixin = isInterface && _prototypeProvider != null && Script.Write<int>("this._prototypeProvider.length") >= 1;
                //AppDomain.GlobalTypeRegistry[_metadata.FullName] = this;
                ////dont try so set inner types, they are managed and readonly within the containing type
                //if (!_metadata.Flags.TypeHasFlag(TypeFlagsModel.IsNested))
                //    AppDomain.GlobalPrototypeRegistry.SetNested(_scriptFullName.NativeReplace("<", "$").NativeReplace(",", "$").NativeReplace(">", "$"), isGenericDefinition || isInterfaceMixin ? _prototypeProvider.As<TypePrototype>() : _prototype);
                //AppDomain.SetupDefaults(this);

                //var fn = FullName;
                //if (fn != null)
                //    AppDomain.GlobalPrototypeRegistry.SetNested(fn, _prototype);
                //AppDomain.GlobalPrototypeRegistry[_metadata.FullName] = _prototype;
                //AppDomain.GlobalTypeRegistry[_metadata.FullName] = this;
            }
        }


        internal void StaticInitialize()
        {
            if (_prototype != null)
            {
                if (Script.IsDefined(_prototype[Constants.StaticInitializerName]))
                    _prototype.StaticInitializeMembers();
                if (Script.IsDefined(_prototype[Constants.StaticConstructorName]))
                    _prototype.StaticConstructor();
            }
        }
        internal void Complete()
        {
            StaticInitialize();
        }

        static bool MemberFilter(MemberInfo i, BindingFlags bindingAttr, Type[]? parameterTypes = null)
        {
            if (i.IsPublic && bindingAttr.TypeHasFlag(BindingFlags.Public))
                return true;
            if (i.IsPrivate && bindingAttr.TypeHasFlag(BindingFlags.NonPublic))
                return true;
            if (i.IsStatic && bindingAttr.TypeHasFlag(BindingFlags.Static))
                return true;
            if (!i.IsStatic && bindingAttr.TypeHasFlag(BindingFlags.Instance))
                return true;
            if (i.MemberType == MemberTypes.Method && bindingAttr.TypeHasFlag(BindingFlags.InvokeMethod))
            {
                if (parameterTypes == null)
                {
                    return true;
                }
                else
                {
                    if (parameterTypes.Length == i.As<RuntimeMethodInfo>().GetParameters().Length)
                    {
                        for (int ip = 0; ip < parameterTypes.Length; ip++)
                        {
                            if (parameterTypes[ip] != i.As<MethodInfo>().GetParameters()[ip].ParameterType)
                                return false;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        [Name(Constants.TypeIsAssignableName)]
        internal static bool IsAssignableInternal(RuntimeType lhs, RuntimeType rhs)
        {
            if (rhs == lhs || lhs.FullName == rhs.FullName)
                return true;
            if (Script.IsDefined(rhs._model.BaseType))
            {
                var btfn = GetTypeFromHandle(rhs._model.BaseType!.Value);
                if (btfn != null && IsAssignableInternal(lhs, btfn))
                    return true;
            }
            if (Script.IsDefined(rhs._model.Interfaces))
            {
                for (int i = 0; i < rhs._model.Interfaces!.Length; i++)
                {
                    var btfn = GetTypeFromHandle(rhs._model.Interfaces[i]);
                    if (btfn != null && IsAssignableInternal(lhs, btfn))
                        return true;
                }
            }
            return false;
        }

        [Name(Constants.TypeIsAssignableName)]
        internal static bool IsSubClassOfInternal(RuntimeType child, RuntimeType @base)
        {
            if (@base == child || child.FullName == @base.FullName)
                return true;
            if (Script.IsDefined(@base._model.BaseType))
            {
                var btfn = GetTypeFromHandle(@base._model.BaseType!.Value);
                if (btfn != null && IsSubClassOfInternal(child, btfn))
                    return true;
            }
            if (Script.IsDefined(@base._model.Interfaces))
            {
                for (int i = 0; i < @base._model.Interfaces!.Length; i++)
                {
                    var btfn = GetTypeFromHandle(@base._model.Interfaces[i]);
                    if (btfn != null && IsSubClassOfInternal(child, btfn))
                        return true;
                }
            }
            return false;
        }

        internal MemberInfo[] GetMembersInternal(
             MemberTypes memberTypes,
             BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance,
             string? name = null,
             Type[]? parameterTypes = null)
        {

            MemberInfo[] Filter(MemberInfo[] info)
            {
                if (name != null)
                {
                    info = info.Filter(i => i.Name == name);
                }
                info = info.Filter(i =>
                {
                    return MemberFilter(i, bindingAttr, parameterTypes);
                });
                return info;
            }
            MemberInfo[] members = [];
            if (memberTypes.TypeHasFlag(MemberTypes.Constructor))
            {
                members = members.ArrayConcat(Filter(_constructors)).As<MemberInfo[]>();
            }
            if (memberTypes.TypeHasFlag(MemberTypes.Field))
            {
                members = members.ArrayConcat(Filter(_fields)).As<MemberInfo[]>();
            }
            if (memberTypes.TypeHasFlag(MemberTypes.Property))
            {
                members = members.ArrayConcat(Filter(_properties)).As<MemberInfo[]>();
            }
            if (memberTypes.TypeHasFlag(MemberTypes.Event))
            {
                members = members.ArrayConcat(Filter(_events)).As<MemberInfo[]>();
            }
            if (memberTypes.TypeHasFlag(MemberTypes.Method))
            {
                members = members.ArrayConcat(Filter(_methods)).As<MemberInfo[]>();
            }
            return members;
        }

        internal MemberInfo? GetMemberInternal(ReflectionHandleModel memberHandle)
        {
            return (MemberInfo?)_constructors.ArrayFirstOrDefault(c => c._miMetadata.Handle.Member == memberHandle.Member) ??
               (MemberInfo?)_fields.ArrayFirstOrDefault(c => c._miMetadata.Handle.Member == memberHandle.Member) ??
               (MemberInfo?)_properties.ArrayFirstOrDefault(c => c._miMetadata.Handle.Member == memberHandle.Member) ??
               (MemberInfo?)_events.ArrayFirstOrDefault(c => c._miMetadata.Handle.Member == memberHandle.Member) ??
               (MemberInfo?)_methods.ArrayFirstOrDefault(c => c._miMetadata.Handle.Member == memberHandle.Member);
        }

        public static RuntimeType? GetTypeFromHandle(ReflectionHandleModel typeHandle)
        {
            return AppDomain.GetType(typeHandle);
        }

        public static string? GetTypeNameFromHandle(uint typeHandle)
        {
            var assemblyHandle = typeHandle.AssemblyHandle();
            var metadata = AppDomain._reflectionMetadata[assemblyHandle.As<uint>()];
            if (Script.IsUndefined(metadata))
                return null;
            var name = metadata.TypeNames[typeHandle.TypeHandle()];
            if (Script.IsUndefined(name))
                return null;
            return name;
        }

        //public static Type? GetType(AssemblyModel assembly, int typeIndex)
        //{
        //    var typeName = assembly.TypeNames[typeIndex];
        //    return GetType(typeName);
        //}

        //public static string GetTypeName(AssemblyModel assembly, int typeIndex)
        //{
        //    var typeName = assembly.TypeNames[typeIndex];
        //    return typeName;
        //}

        RuntimeType MakeGenericTypeInternal(params RuntimeType[] typeArguments)
        {
            if (_prototypeProvider == null)
                throw new InvalidOperationException();
            if (_model.GenericParameterCount != typeArguments.Length)
                throw new ArgumentException("Incorrect number of type arguments supplied");
            var newPrototype = _prototypeProvider(typeArguments.Map(t => t.As<RuntimeType>()._prototype!), null);
            // Script.Apply<TypePrototype>(null, _prototype, typeArguments.Map(t => t._prototype));
            return MakeGenericTypeInternal(typeArguments, newPrototype);
        }

        internal RuntimeType MakeGenericTypeInternal(RuntimeType[] typeArguments, TypePrototype prototype)
        {
            if (_model.GenericParameterCount != typeArguments.Length)
                throw new ArgumentException("Incorrect number of type arguments supplied");
            var newTypeModel = Script.JSONParse<TypeModel>(Script.JSONStringify(_model));
            var t = RuntimeType.Create(_assembly, prototype, newTypeModel, _scriptFullName);
            t._parentGenericTypeDefinition = _parentGenericTypeDefinition ?? this;
            t._typeArguments = typeArguments;
            return t;
        }

        internal Type[] GetGenericArgumentsInternalImpl()
        {
            if (!IsGenericType)
                return Array.Empty<Type>();
            if (_typeArguments != null)
                return _typeArguments;
            return _typeArguments = _model.GenericArguments?.Map((arg, i, all) =>
            {
                var argType = GetTypeFromHandle(arg);
                var constraint = Script.IsDefined(_model.GenericConstraints) ? _model.GenericConstraints![i] : null;//?.Filter(c => c.ParameterName == arg)[0];
                bool firstConstraintIsClass;
                if (constraint != null && constraint.TypeConstraints?.Length > 0)
                {
                    var firstConstraintType = GetTypeFromHandle(constraint.TypeConstraints[0]);
                    firstConstraintIsClass = firstConstraintType != null && !firstConstraintType.IsInterface;
                }
                else
                {
                    firstConstraintIsClass = false;
                }
                var model = new TypeModel()
                {
                    //Name = arg,
                    Handle = new ReflectionHandleModel { Value = 0 },// arg,
                    BaseType = constraint != null && firstConstraintIsClass ? constraint.TypeConstraints?[0] : null,
                    Interfaces = constraint != null ? (firstConstraintIsClass ? constraint.TypeConstraints!.Slice(1).As<ReflectionHandleModel[]>() : constraint.TypeConstraints) : null,
                };
                var type = Create(null, Script.Write<TypePrototype>("$.System.GenericTypeArgument" /*+ nameof(GenericTypeArgument)*/), model, _scriptFullName);
                type._genericParameterPosition = i;
                type._typeConstraints = constraint?.TypeConstraints?.Map(c => GetTypeFromHandle(c) ?? throw new InvalidOperationException());
                type._constraintModel = constraint;
                return type;
            }) ?? Array.Empty<RuntimeType>();
        }
        //static SimpleDictionary<RuntimeType> types = new SimpleDictionary<RuntimeType>();

        // Returns the type from which the current type directly inherits from (without reflection quirks).
        // The parent type is null for interfaces, pointers, byrefs and generic parameters.
        [NetJs.MemberReplace(nameof(GetParentType) + "(QCallTypeHandle, ObjectHandleOnStack)")]
        private static void GetParentTypeImpl(QCallTypeHandle type, ObjectHandleOnStack res)
        {
            var runtimeType = RuntimeHelpers.QCallTypeHandleToRuntimeType(type);
            if (runtimeType._model.Kind == TypeKindModel.Class || runtimeType._model.Kind == TypeKindModel.Struct)
            {
                if (runtimeType._model.BaseType != null)
                {
                    res.GetObjectHandleOnStack<RuntimeType>() = AppDomain.GlobalTypeRegistry[(int)runtimeType._model.BaseType.Value.AssemblyAndType];
                }
            }
            RuntimeHelpers.GetObjectHandleOnStack<RuntimeType?>(res) = null;
        }

        [NetJs.MemberReplace(nameof(GetCorrespondingInflatedMethod))]
        private static MemberInfo GetCorrespondingInflatedMethodImpl(QCallTypeHandle type, MemberInfo generic)
        {
            return generic;
        }

        [NetJs.MemberReplace(nameof(make_array_type))]
        private static void make_array_typeImpl(QCallTypeHandle type, int rank, ObjectHandleOnStack res)
        {
            var tp = type.QCallTypeHandleToRuntimeType();
            var genericArray = AppDomain.GlobalTypeRegistry.GetNested("System.Array<>");
            var genericArrayType = genericArray.MakeGenericTypeInternal([tp]);
            genericArrayType._arrayTypeRank = rank;
            genericArrayType._arrayElementType = tp;
            res.GetObjectHandleOnStack<Type>() = genericArrayType;
        }

        [NetJs.MemberReplace(nameof(make_byref_type))]
        private static void make_byref_typeImpl(QCallTypeHandle type, ObjectHandleOnStack res)
        {
            var tp = type.QCallTypeHandleToRuntimeType();
            var genericArray = AppDomain.GlobalTypeRegistry.GetNested("System.RefOrPointer<>");
            var genericArrayType = genericArray.MakeGenericTypeInternal([tp]);
            res.GetObjectHandleOnStack<Type>() = genericArrayType;
        }

        [NetJs.MemberReplace(nameof(make_pointer_type))]
        private static void make_pointer_typeImpl(QCallTypeHandle type, ObjectHandleOnStack res)
        {
            var tp = type.QCallTypeHandleToRuntimeType();
            var genericArray = AppDomain.GlobalTypeRegistry.GetNested("System.RefOrPointer<>");
            var genericArrayType = genericArray.MakeGenericTypeInternal([tp]);
            res.GetObjectHandleOnStack<Type>() = genericArrayType;
        }

        [NetJs.MemberReplace(nameof(MakeGenericType))]
        private static void MakeGenericTypeImpl(Type gt, Type[] types, ObjectHandleOnStack res)
        {
            var genericArrayType = gt.As<RuntimeType>().MakeGenericTypeInternal(types.As<RuntimeType[]>());
            res.GetObjectHandleOnStack<Type>() = genericArrayType;
        }

        [NetJs.MemberReplace(nameof(GetMethodsByName))]
        internal RuntimeMethodInfo[] GetMethodsByNameOverride(string? name, BindingFlags bindingAttr, MemberListType listType, RuntimeType reflectedType)
        {
            return GetMembersInternal(MemberTypes.Method, bindingAttr, name).As<RuntimeMethodInfo[]>();
        }

        [NetJs.MemberReplace(nameof(GetConstructors_internal))]
        private RuntimeConstructorInfo[] GetConstructors_internalOverride(BindingFlags bindingAttr, RuntimeType reflectedType)
        {
            return GetMembersInternal(MemberTypes.Constructor, bindingAttr).As<RuntimeConstructorInfo[]>();
        }

        [NetJs.MemberReplace(nameof(GetPropertiesByName))]
        private RuntimePropertyInfo[] GetPropertiesByNameOverride(string? name, BindingFlags bindingAttr, MemberListType listType, RuntimeType reflectedType)
        {
            return GetMembersInternal(MemberTypes.Property, bindingAttr).As<RuntimePropertyInfo[]>();
        }

        [NetJs.MemberReplace(nameof(GetFields_internal))]
        private RuntimeFieldInfo[] GetFields_internalOverride(string? name, BindingFlags bindingAttr, MemberListType listType, RuntimeType reflectedType)
        {
            return GetMembersInternal(MemberTypes.Method, bindingAttr, name).As<RuntimeFieldInfo[]>();
        }

        [NetJs.MemberReplace(nameof(GetEvents_internal))]
        private RuntimeFieldInfo[] GetEvents_internalOverride(string? name, BindingFlags bindingAttr, MemberListType listType, RuntimeType reflectedType)
        {
            return GetMembersInternal(MemberTypes.Event, bindingAttr, name).As<RuntimeFieldInfo[]>();
        }

        [NetJs.MemberReplace(nameof(GetInterfaces))]
        private static void GetInterfacesImpl(QCallTypeHandle type, ObjectHandleOnStack res)
        {
            var mtype = type.QCallTypeHandleToRuntimeType();
            res.GetObjectHandleOnStack<RuntimeType[]?>() = mtype._model.Interfaces?.Map(i => RuntimeType.GetTypeFromHandle(i) ?? throw new InvalidOperationException()) ?? [];
        }

        [NetJs.MemberReplace(nameof(GetNestedTypes_internal))]
        private RuntimeType[] GetNestedTypes_internalOverride(string? displayName, BindingFlags bindingAttr, MemberListType listType)
        {
            return _model.NestedTypes?.Map(i => RuntimeType.GetTypeFromHandle(i) ?? throw new InvalidOperationException())
                .Filter(nt => (displayName == null || nt.Name.Contains(displayName)) && MemberFilter(nt, bindingAttr, null)) ?? [];
        }

        [NetJs.MemberReplace(nameof(GetDeclaringType))]
        private static void GetDeclaringTypeImpl(QCallTypeHandle type, ObjectHandleOnStack res)
        {
            var mtype = type.QCallTypeHandleToRuntimeType();
            res.GetObjectHandleOnStack<Type?>() = mtype._model.DeclaringType != null ? RuntimeType.GetTypeFromHandle(mtype._model.DeclaringType.Value) : null;
        }

        [NetJs.MemberReplace(nameof(GetName))]
        private static void GetNameImpl(QCallTypeHandle type, ObjectHandleOnStack res)
        {
            var mtype = type.QCallTypeHandleToRuntimeType();
            res.GetObjectHandleOnStack<string?>() = mtype.InternalName;
        }

        [NetJs.MemberReplace(nameof(GetNamespace))]
        private static void GetNamespaceImpl(QCallTypeHandle type, ObjectHandleOnStack res)
        {
            var mtype = type.QCallTypeHandleToRuntimeType();
            res.GetObjectHandleOnStack<string?>() = mtype.InternalNamespace;
        }

        [NetJs.MemberReplace(nameof(GetInterfaceMapData))]
        private static void GetInterfaceMapDataImpl(QCallTypeHandle t, QCallTypeHandle iface, out MethodInfo[] targets, out MethodInfo[] methods)
        {
            var targetType = t.QCallTypeHandleToRuntimeType();
            var interfaceType = iface.QCallTypeHandleToRuntimeType();
            targets = targetType.GetMembersInternal(MemberTypes.Method).As<MethodInfo[]>();
            methods = interfaceType.GetMembersInternal(MemberTypes.Method).As<MethodInfo[]>();
        }

        [NetJs.MemberReplace(nameof(GetPacking))]
        private static void GetPackingImpl(QCallTypeHandle type, out int packing, out int size)
        {
            var mtype = type.QCallTypeHandleToRuntimeType();
            packing = 0;
            size = 0;
        }

        [NetJs.MemberReplace(nameof(CreateInstanceInternal))]
        private static object CreateInstanceInternalImpl(QCallTypeHandle type)
        {
            var mtype = type.QCallTypeHandleToRuntimeType();
            return NetJs.Script.Write<object>("(new mtype._prototype).$ctor()");
        }

        [NetJs.MemberReplace(nameof(GetDeclaringMethod))]
        private static void GetDeclaringMethodImpl(QCallTypeHandle type, ObjectHandleOnStack res)
        {
            var mtype = type.QCallTypeHandleToRuntimeType();
            res.GetObjectHandleOnStack<MethodInfo?>() = null;
        }

        [NetJs.MemberReplace(nameof(getFullName))]
        internal static void getFullNameImpl(QCallTypeHandle type, ObjectHandleOnStack res, bool full_name, bool assembly_qualified)
        {
            var mtype = type.QCallTypeHandleToRuntimeType();
            var name = assembly_qualified ? mtype.InternalAssemblyQualifiedName : full_name ? mtype.InternalFullName : mtype.Name;
            res.GetObjectHandleOnStack<string?>() = name;
        }

        [NetJs.MemberReplace(nameof(GetGenericArgumentsInternal))]
        private static void GetGenericArgumentsInternalImpl(QCallTypeHandle type, ObjectHandleOnStack res, bool runtimeArray)
        {
            var mtype = type.QCallTypeHandleToRuntimeType();
            res.GetObjectHandleOnStack<Type[]>() = mtype.GetGenericArgumentsInternalImpl();
        }

        [NetJs.MemberReplace(nameof(GetGenericParameterPosition))]
        private static int GetGenericParameterPositionImpl(QCallTypeHandle type)
        {
            var mtype = type.QCallTypeHandleToRuntimeType();
            return mtype._genericParameterPosition;
        }

        [NetJs.MemberReplace(nameof(IsUnmanagedFunctionPointerInternal))]
        internal static bool IsUnmanagedFunctionPointerInternalImpl(QCallTypeHandle type)
        {
            return false;
        }

        [NetJs.MemberReplace(nameof(FunctionPointerReturnAndParameterTypes))]
        internal static IntPtr FunctionPointerReturnAndParameterTypesImpl(QCallTypeHandle type)
        {
            throw new NotImplementedException();
        }

        [NetJs.MemberReplace(nameof(GetFunctionPointerTypeModifiers))]
        internal static Type[] GetFunctionPointerTypeModifiersImpl(QCallTypeHandle type, int position, bool optional)
        {
            throw new NotImplementedException();
        }

        [NetJs.MemberReplace(nameof(GetCallingConventionFromFunctionPointerInternal))]
        internal static byte GetCallingConventionFromFunctionPointerInternalImpl(QCallTypeHandle type)
        {
            throw new NotImplementedException();
        }

        [NetJs.MemberReplace(nameof(GetGenericParameterConstraints))]
        public Type[] GetGenericParameterConstraintsOverride()
        {
            if (!IsGenericParameter)
                throw new InvalidOperationException(SR.Arg_NotGenericParameter);

            return _typeConstraints ?? Type.EmptyTypes;
        }

        [NetJs.MemberReplace(nameof(GetGenericParameterAttributes))]
        private GenericParameterAttributes GetGenericParameterAttributesOverride()
        {
            var flags = GenericParameterAttributes.None;
            if (_constraintModel != null)
            {
                if (_constraintModel.Flags.TypeHasFlag(GenericConstraintFlagsModel.HasNewConstraint))
                {
                    flags |= GenericParameterAttributes.DefaultConstructorConstraint;
                }
                if (_constraintModel.Flags.TypeHasFlag(GenericConstraintFlagsModel.HasClassConstraint))
                {
                    flags |= GenericParameterAttributes.ReferenceTypeConstraint;
                }
                if (_constraintModel.Flags.TypeHasFlag(GenericConstraintFlagsModel.HasStructConstraint))
                {
                    flags |= GenericParameterAttributes.NotNullableValueTypeConstraint;
                }
                //if (_constraintModel.Flags.TypeHasFlag(GenericConstraintFlagsModel.HasUnmanagedConstraint))
                //{
                //    flags |= GenericParameterAttributes.;
                //}
            }
            return flags;
        }
    }
}