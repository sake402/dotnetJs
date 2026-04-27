using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    [NetJs.StaticCallConvention]
    [NetJs.ExternalInterfaceImplementation(typeof(ArrayEnumerator))]
    public abstract partial class Array
    {
        [NetJs.InlineConst]
        public const string InterfaceImplementationName = "$implements";
        [NetJs.InlineConst]
        public const string ElementTypeName = "$elementType";
        [NetJs.InlineConst]
        public const string SizesName = "$sizes";
        [NetJs.InlineConst]
        public const string LowerBoundsName = "$lb";

        //Type ElementType
        //{
        //    get => this[ElementTypeName].As<TypePrototype>()?.Type;
        //}

        [NetJs.MemberReplace(nameof(Length))]
        [NetJs.StaticCallConvention(false)]
        public extern int LengthImpl
        {
            [NetJs.Template("{this}.length")]
            get;
        }

        [NetJs.MemberReplace(nameof(NativeLength))]
        [NetJs.StaticCallConvention(false)]
        [CLSCompliant(false)]
        public extern nuint NativeLengthImpl
        {
            [NetJs.Template("{this}.length")]
            get;
        }

        [NetJs.MemberReplace(nameof(LongLength))]
        [NetJs.StaticCallConvention(false)]
        public extern int LongLengthImpl
        {
            [NetJs.Template("{this}.length")]
            get;
        }

        [NetJs.MemberReplace(nameof(Rank))]
        public int RankImpl
        {
            //[dotnetJs.Template("{assembly.}System.Array." + nameof(_GetRank) + "({this})")]
            //get;
            get
            {
                var sz = this[SizesName].As<int[]>();
                if (NetJs.Script.IsDefined(sz))
                    return sz.Length;
                return 1;
            }
        }

        [NetJs.MemberReplace(nameof(Clone))]
        public object CloneImpl()
        {
            var clone = this.ArrayClone();
            if (NetJs.Script.IsDefined(this[ElementTypeName]))
                clone[ElementTypeName] = this[ElementTypeName];
            if (NetJs.Script.IsDefined(this[SizesName]))
                clone[SizesName] = this[SizesName];
            if (NetJs.Script.IsDefined(this[LowerBoundsName]))
                clone[LowerBoundsName] = this[LowerBoundsName];
            return clone;
        }

        [NetJs.Name(NetJs.Constants.IsTypeName)]
        public static bool Is(object? instance)
        {
            return NetJs.Script.Write<bool>("window.Array.isArray(instance)");
        }

        [NetJs.StaticCallConvention]
        [NetJs.Name("$Read")]
        protected object? Read(params int[] indices)
        {
            if (indices == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.indices);
            var rank = Rank;
            if (rank != indices.Length)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankIndices);
            if (rank == 1)
            {
                unchecked
                {
                    var index = indices[0];
                    //if (NetJs.Script.IsUndefined(this[ElementTypeName])) //Not a dotnet array, morelikely a pure js array, so we can just index it directly
                    return this[index];
                    //return InternalGetValue(index);
                }
            }
            return InternalGetValue(GetFlattenedIndex(indices));
        }

        [NetJs.StaticCallConvention]
        [NetJs.Name("$Write")]
        protected void Write(object? value, params int[] indices)
        {
            if (indices == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.indices);
            var rank = Rank;
            if (rank != indices.Length)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankIndices);
            if (rank == 1)
            {
                unchecked
                {
                    var index = indices[0];
                    //if (NetJs.Script.IsUndefined(this[ElementTypeName])) //Not a dotnet array, more likely a pure js array, so we can just index it directly
                    this[index] = value;
                    //else
                    //InternalSetValue(value, index);
                }
            }
            else
                InternalSetValue(value, GetFlattenedIndex(indices));
        }

        [NetJs.Unbox(false)]
        public extern object? this[int index]
        {
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Read) + ".call({this}, [{index}])")]
            [NetJs.Template("{this}[{index}]", "unchecked")]
            get;
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Write) + ".call({this}, {value}, [{index}])")]
            [NetJs.Template("{this}[{index}] = {value}", "unchecked")]
            set;
        }

        //public extern object this[Range range]
        //{
        //    [dotnetJs.External]
        //    [dotnetJs.Template("{assembly.}System.Array." + nameof(_Range) + "({this}, {range})")]
        //    get;
        //}

        //public extern object this[Index index]
        //{
        //    [dotnetJs.External]
        //    [dotnetJs.Template("{assembly.}System.Array." + nameof(_Index) + "({this}, {index})")]
        //    get;
        //}        

        [NetJs.Unbox(false)]
        public extern object? this[int index1, int index2]
        {
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Read) + ".call({this}, [{index1}, {index2}])")]
            get;
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Write) + ".call({this}, {value}, [{index1}, {index2}])")]
            set;
        }

        [NetJs.Unbox(false)]
        public extern object? this[int index1, int index2, int index3]
        {
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Read) + ".call({this}, [{index1}, {index2}, {index3}])")]
            get;
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Write) + ".call({this}, {value}, [{index1}, {index2}, {index3}])")]
            set;
        }

        [NetJs.Unbox(false)]
        public extern object? this[int index1, int index2, int index3, int index4]
        {
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Read) + ".call({this}, [{index1}, {index2}, {index3}, {index4}])")]
            get;
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Write) + ".call({this}, {value}, [{index1}, {index2}, {index3}, {index4}])")]
            set;
        }

        [NetJs.Unbox(false)]
        public extern object? this[int index1, int index2, int index3, int index4, int index5]
        {
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Read) + ".call({this}, [{index1}, {index2}, {index3}, {index4}, {index5}])")]
            get;
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Write) + ".call({this}, {value}, [{index1}, {index2}, {index3}, {index4}, {index5}])")]
            set;
        }

        internal static Type GetArrayType(Array array)
        {
            var et = array[ElementTypeName].As<RuntimeType?>();
            if (NetJs.Script.IsUndefined(et))
            {
                et = null;
            }
            var elementPrototype = et?._prototype ?? typeof(object).As<RuntimeType>()._prototype;
            //var prototype = elementType._prototype;
            return NetJs.Script.Write<Type>($"$.{NetJs.Constants.TypeOf}($.{NetJs.Constants.TypeArray}(elementPrototype))");
            //return typeof(Array<>).MakeGenericType(elementType);
        }

        internal static void AddMetadata(Array arr, Type elementType, int[]? sizes = null, int[]? lowerBounds = null)
        {
            arr[SizesName] = sizes ?? [arr.Length];
            arr[ElementTypeName] = elementType;
            if (lowerBounds != null)
            {
                arr[LowerBoundsName] = lowerBounds;
            }
        }

        internal static Array CreateNested(RuntimeType type, int[] sizes, int[]? lowerBounds, NetJs.Union<object, object[]>? fill, int depth)
        {
            unchecked
            {
                Array arr = NetJs.Script.Write<Array>("window.Array(sizes[depth])"); //make sure we dont create this Array class itself again
                if (depth == 0)
                {
                    AddMetadata(arr, type, sizes, lowerBounds);
                }
                if (depth < sizes.Length - 1)
                {
                    for (int i = 0; i < sizes[depth]; i++)
                    {
                        var innerArray = CreateNested(type, sizes, lowerBounds, fill, depth + 1);
                        arr[i] = innerArray;
                    }
                }
                else
                {
                    if (NetJs.Script.IsDefined(fill))
                    {
                        if (NetJs.Script.Write<bool>("window.Array.isArray(fill)")/* Script.IsArray(fill)*/)
                        {
                            fill.As<Array>().CopyTo(arr, 0);
                        }
                        else
                        {
                            for (int i = 0; i < sizes[depth]; i++)
                            {
                                arr[i] = fill;
                            }
                        }
                    }
                    else
                    {
                        var prototype = type._prototype;
                        //For struct, non primitive types, make sure we create different instance for each array item
                        var defaultValue = type._model.Flags.TypeHasFlag(TypeFlagsModel.IsValueType) && !type._model.Flags.TypeHasFlag(TypeFlagsModel.IsPrimitive) ?
                            NetJs.Script.Undefined :
                            NetJs.Script.Write<object>($"$.{NetJs.Constants.DefaultTypeName}(prototype)");
                        for (int i = 0; i < sizes[depth]; i++)
                        {
                            arr[i] = defaultValue ?? NetJs.Script.Write<object>($"$.{NetJs.Constants.DefaultTypeName}(prototype)");
                        }
                    }
                }
                return arr;
            }
        }


        [NetJs.MemberReplace(nameof(InternalCreate))]
        private static unsafe void InternalCreateImpl(ref Array? result, IntPtr elementType, int rank, int* lengths, int* lowerBounds)
        {
            unchecked
            {
                var sizes = NetJs.Script.Write<int[]>("window.Array(rank)");
                var lb = lowerBounds != null ? NetJs.Script.Write<int[]>("window.Array(rank)") : null;
                for (int i = 0; i < rank; i++)
                {
                    sizes[i] = lengths[i];
                    if (lowerBounds != null)
                    {
                        lb![i] = lowerBounds[i];
                    }
                }
                var type = AppDomain.GetType((uint)elementType) ?? throw new InvalidOperationException();
                var arr = CreateNested(type, sizes, lb, null, 0);
                result = arr;
            }
        }

        [NetJs.MemberReplace(nameof(GetCorElementTypeOfElementTypeInternal))]
        private static CorElementType GetCorElementTypeOfElementTypeInternalImpl(ObjectHandleOnStack arr)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            var elementType = marr[ElementTypeName].As<RuntimeType>();
            return RuntimeTypeHandle.GetCorElementType(new QCallTypeHandle(ref elementType));
        }

        [NetJs.MemberReplace(nameof(IsValueOfElementTypeInternal))]
        private static bool IsValueOfElementTypeInternalImpl(ObjectHandleOnStack arr, ObjectHandleOnStack obj)
        {
            var array = arr.GetObjectHandleOnStack<Array>();
            var value = obj.GetObjectHandleOnStack<object>();
            var elementType = array[ElementTypeName].As<RuntimeType>();
            return RuntimeTypeHandle.IsInstanceOfType(new QCallTypeHandle(ref elementType), value);
        }

        [NetJs.MemberReplace(nameof(CanChangePrimitive))]
        private static bool CanChangePrimitiveImpl(ObjectHandleOnStack srcType, ObjectHandleOnStack dstType, bool reliable)
        {
            var src = srcType.GetObjectHandleOnStack<RuntimeType>();
            var dst = dstType.GetObjectHandleOnStack<RuntimeType>();
            return src.IsPrimitive && dst.IsPrimitive;
        }

        [NetJs.MemberReplace(nameof(FastCopy))]
        internal static bool FastCopyImpl(ObjectHandleOnStack source, int source_idx, ObjectHandleOnStack dest, int dest_idx, int length)
        {
            var sourceArray = source.GetObjectHandleOnStack<Array>();
            var destinationArray = dest.GetObjectHandleOnStack<Array>();
            if (source_idx < dest_idx && sourceArray == destinationArray)
            {
                while (--length >= 0)
                {
                    destinationArray[dest_idx + length] = sourceArray[source_idx + length];
                }
            }
            else
            {
                for (var i = 0; i < length; i++)
                {
                    destinationArray[dest_idx + i] = sourceArray[source_idx + i];
                }
            }
            return true;
        }

        [NetJs.MemberReplace(nameof(GetLengthInternal))]
        private static int GetLengthInternalImpl(ObjectHandleOnStack arr, int dimension)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            var sizes = marr[SizesName].As<int[]>();
            if (NetJs.Script.IsUndefinedOrNull(sizes) && dimension == 0)
                return marr.Length;
            unchecked
            {
                return sizes[dimension];
            }
        }

        [NetJs.MemberReplace(nameof(GetLowerBoundInternal))]
        private static int GetLowerBoundInternalImpl(ObjectHandleOnStack arr, int dimension)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            var bounds = marr[LowerBoundsName].As<int[]?>();
            if (NetJs.Script.IsUndefinedOrNull(bounds))
                return 0;
            unchecked
            {
                return bounds![dimension];
            }
        }

        // CAUTION! No bounds checking!
        [NetJs.MemberReplace(nameof(GetGenericValue_icall) + "<>")]
        private static void GetGenericValue_icallImpl<T>(ObjectHandleOnStack self, int pos, out T value)
        {
            var marr = self.GetObjectHandleOnStack<Array>();
            unchecked
            {
                value = marr[pos].As<T>();
            }
        }

        // CAUTION! No bounds checking!
        [NetJs.MemberReplace(nameof(GetValueImpl))]
        private static void GetValueImplImpl(ObjectHandleOnStack arr, ObjectHandleOnStack res, int pos)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            unchecked
            {
                res.GetObjectHandleOnStack<object?>() = marr[pos];
            }
        }

        // CAUTION! No bounds checking!
        [NetJs.MemberReplace(nameof(SetGenericValue_icall) + "<>")]
        private static void SetGenericValue_icallImpl<T>(ObjectHandleOnStack arr, int pos, ref T value)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            unchecked
            {
                marr[pos] = value;
            }
        }

        // CAUTION! No bounds checking!
        [NetJs.MemberReplace(nameof(SetValueImpl))]
        private static void SetValueImplImpl(ObjectHandleOnStack arr, ObjectHandleOnStack value, int pos)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            unchecked
            {
                marr[pos] = value.GetObjectHandleOnStack<object?>();
            }
        }

        [NetJs.MemberReplace(nameof(InitializeInternal))]
        private static void InitializeInternalImpl(ObjectHandleOnStack arr)
        {

        }

        // CAUTION! No bounds checking!
        [NetJs.MemberReplace(nameof(SetValueRelaxedImpl))]
        private static void SetValueRelaxedImplImpl(ObjectHandleOnStack arr, ObjectHandleOnStack value, int pos)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            unchecked
            {
                marr[pos] = value.GetObjectHandleOnStack<object?>();
            }
        }

    }

    //Class only defined for generator use
    //This class makes indexing a typed array work
    //[NetJs.External]
    public abstract class Array<T> : Array
    {
        [NetJs.Unbox(false)]
        public new extern T this[int index]
        {
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Read) + ".call({this}, [{index}])")]
            [NetJs.Template("{this}[{index}]", "unchecked")]
            get;
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Write) + ".call({this}, {value}, [{index}])")]
            [NetJs.Template("{this}[{index}] = {value}", "unchecked")]
            set;
        }

        //public extern T this[Range range]
        //{
        //    [dotnetJs.External]
        //    [dotnetJs.Template("{assembly.}System.Array." + nameof(_Range) + "({this}, {range})")]
        //    get;
        //}

        //public new extern T this[Index index]
        //{
        //    [dotnetJs.External]
        //    [dotnetJs.Template("{assembly.}System.Array." + nameof(_Index) + "({this}, {index})")]
        //    get;
        //}

        [NetJs.Unbox(false)]
        public new extern T this[int index1, int index2]
        {
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Read) + ".call({this}, [{index1}, {index2}])")]
            get;
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Write) + ".call({this}, {value}, [{index1}, {index2}])")]
            set;
        }

        [NetJs.Unbox(false)]
        public new extern T this[int index1, int index2, int index3]
        {
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Read) + ".call({this}, [{index1}, {index2}, {index3}])")]
            get;
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Write) + ".call({this}, {value}, [{index1}, {index2}, {index3}])")]
            set;
        }

        [NetJs.Unbox(false)]
        public new extern T this[int index1, int index2, int index3, int index4]
        {
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Read) + ".call({this}, [{index1}, {index2}, {index3}, {index4}])")]
            get;
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Write) + ".call({this}, {value}, [{index1}, {index2}, {index3}, {index4}])")]
            set;
        }

        [NetJs.Unbox(false)]
        public new extern T this[int index1, int index2, int index3, int index4, int index5]
        {
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Read) + ".call({this}, [{index1}, {index2}, {index3}, {index4}, {index5}])")]
            get;
            [NetJs.Template("{assembly.}System.Array.$" + nameof(Write) + ".call({this}, {value}, [{index1}, {index2}, {index3}, {index4}, {index5}])")]
            set;
        }
    }
}
