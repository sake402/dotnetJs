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
        public extern int IntrinsicLength
        {
            [NetJs.Template("{this}.length")]
            get;
        }

        [NetJs.MemberReplace(nameof(NativeLength))]
        [NetJs.StaticCallConvention(false)]
        [CLSCompliant(false)]
        public extern nuint IntrinsicNativeLength
        {
            [NetJs.Template("{this}.length")]
            get;
        }

        [NetJs.MemberReplace(nameof(LongLength))]
        [NetJs.StaticCallConvention(false)]
        public extern int IntrinsicLongLength
        {
            [NetJs.Template("{this}.length")]
            get;
        }

        [NetJs.MemberReplace(nameof(Rank))]
        public int IntrinsicRank
        {
            //[dotnetJs.Template("{global.}System.Array." + nameof(_GetRank) + "({this})")]
            //get;
            get
            {
                var sz = this[SizesName].As<int[]>();
                if (NetJs.Script.IsDefined(sz))
                    return sz.Length;
                return 1;
            }
        }

        [NetJs.Name(NetJs.Constants.IsTypeName)]
        public static bool Is(object? instance)
        {
            return NetJs.Script.Write<bool>("window.Array.isArray(instance)");
        }

        [NetJs.Unbox(false)]
        public extern object? this[int index]
        {
            [NetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index}])")]
            [NetJs.Template("{this}[{index}]", "unchecked")]
            get;
            [NetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index}])")]
            [NetJs.Template("{this}[{index}] = {value}", "unchecked")]
            set;
        }

        //public extern object this[Range range]
        //{
        //    [dotnetJs.External]
        //    [dotnetJs.Template("{global.}System.Array." + nameof(_Range) + "({this}, {range})")]
        //    get;
        //}

        //public extern object this[Index index]
        //{
        //    [dotnetJs.External]
        //    [dotnetJs.Template("{global.}System.Array." + nameof(_Index) + "({this}, {index})")]
        //    get;
        //}        

        [NetJs.Unbox(false)]
        public extern object? this[int index1, int index2]
        {
            [NetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index1}, {index2}])")]
            get;
            [NetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index1}, {index2}])")]
            set;
        }

        [NetJs.Unbox(false)]
        public extern object? this[int index1, int index2, int index3]
        {
            [NetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index1}, {index2}, {index3}])")]
            get;
            [NetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index1}, {index2}, {index3}])")]
            set;
        }

        [NetJs.Unbox(false)]
        public extern object? this[int index1, int index2, int index3, int index4]
        {
            [NetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index1}, {index2}, {index3}, {index4}])")]
            get;
            [NetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index1}, {index2}, {index3}, {index4}])")]
            set;
        }

        [NetJs.Unbox(false)]
        public extern object? this[int index1, int index2, int index3, int index4, int index5]
        {
            [NetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index1}, {index2}, {index3}, {index4}, {index5}])")]
            get;
            [NetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index1}, {index2}, {index3}, {index4}, {index5}])")]
            set;
        }

        internal static Array _Create(Type type, int[] sizes, int[]? lowerBounds, NetJs.Union<object, object[]>? fill, int depth)
        {
            unchecked
            {
                Array arr = NetJs.Script.Write<Array>("window.Array(sizes[depth])"); //make sure we dont create this Array class itself again
                                                                               //new object[sizes[depth]];
                if (depth == 0)
                {
                    //arr[Constants.ObjectTypeName] = typeof(ArrayInterface<T>);
                    //arr[InterfaceImplementationName] = new ArrayInterface<T>(arr.As<T[]>());
                    arr[SizesName] = sizes;
                    arr[ElementTypeName] = type;
                    if (lowerBounds != null)
                    {
                        arr[LowerBoundsName] = lowerBounds;
                    }
                }
                if (depth < sizes.Length - 1)
                {
                    for (int i = 0; i < sizes[depth]; i++)
                    {
                        var innerArray = _Create(type, sizes, lowerBounds, fill, depth + 1);
                        NetJs.Script.Write("arr[i] = innerArray");
                        //arr[i] = innerArray;
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
                        for (int i = 0; i < sizes[depth]; i++)
                        {
                            arr[i] = NetJs.Script.Write<object>("$.default(type)");
                        }
                    }
                }
                return arr;
            }
        }


        [NetJs.MemberReplace(nameof(InternalCreate))]
        private static unsafe void InternalCreateImpl(ref Array? result, IntPtr elementType, int rank, int* lengths, int* lowerBounds)
        {
            var sizes = new int[rank];
            var lb = lowerBounds != null ? new int[rank] : null;
            for (int i = 0; i < rank; i++)
            {
                sizes[i] = lengths[i];
                if (lowerBounds != null)
                {
                    lb![i] = lowerBounds[i];
                }
            }
            var type = AppDomain.GetType(new ReflectionHandleModel { Value = (uint)elementType }) ?? throw new InvalidOperationException();
            var arr = _Create(type, sizes, lb, null, 0);
            result = arr;
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
            return sizes[dimension];
        }

        [NetJs.MemberReplace(nameof(GetLowerBoundInternal))]
        private static int GetLowerBoundInternalImpl(ObjectHandleOnStack arr, int dimension)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            var bounds = marr[LowerBoundsName].As<int[]?>();
            if (bounds == null)
                return 0;
            return bounds[dimension];
        }

        // CAUTION! No bounds checking!
        [NetJs.MemberReplace(nameof(GetGenericValue_icall))]
        private static void GetGenericValue_icallImpl<T>(ObjectHandleOnStack self, int pos, out T value)
        {
            var marr = self.GetObjectHandleOnStack<Array>();
            value = marr[pos].As<T>();
        }

        // CAUTION! No bounds checking!
        [NetJs.MemberReplace(nameof(GetValueImpl))]
        private static void GetValueImplImpl(ObjectHandleOnStack arr, ObjectHandleOnStack res, int pos)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            res.GetObjectHandleOnStack<object?>() = marr[pos];
        }

        // CAUTION! No bounds checking!
        [NetJs.MemberReplace(nameof(SetGenericValue_icall))]
        private static void SetGenericValue_icallImpl<T>(ObjectHandleOnStack arr, int pos, ref T value)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            marr[pos] = value;
        }

        // CAUTION! No bounds checking!
        [NetJs.MemberReplace(nameof(SetValueImpl))]
        private static void SetValueImplImpl(ObjectHandleOnStack arr, ObjectHandleOnStack value, int pos)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            value.GetObjectHandleOnStack<object?>() = marr[pos];
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
            value.GetObjectHandleOnStack<object?>() = marr[pos];
        }

    }

    //Class only defined for generator use
    //This class makes indexing a typed array work
    [NetJs.External]
    public abstract class Array<T> : Array
    {
        [NetJs.Unbox(false)]
        public new extern T this[int index]
        {
            [NetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index}])")]
            [NetJs.Template("{this}[{index}]", "unchecked")]
            get;
            [NetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index}])")]
            [NetJs.Template("{this}[{index}] = {value}", "unchecked")]
            set;
        }

        //public extern T this[Range range]
        //{
        //    [dotnetJs.External]
        //    [dotnetJs.Template("{global.}System.Array." + nameof(_Range) + "({this}, {range})")]
        //    get;
        //}

        //public new extern T this[Index index]
        //{
        //    [dotnetJs.External]
        //    [dotnetJs.Template("{global.}System.Array." + nameof(_Index) + "({this}, {index})")]
        //    get;
        //}

        [NetJs.Unbox(false)]
        public new extern T this[int index1, int index2]
        {
            [NetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index1}, {index2}])")]
            get;
            [NetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index1}, {index2}])")]
            set;
        }

        [NetJs.Unbox(false)]
        public new extern T this[int index1, int index2, int index3]
        {
            [NetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index1}, {index2}, {index3}])")]
            get;
            [NetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index1}, {index2}, {index3}])")]
            set;
        }

        [NetJs.Unbox(false)]
        public new extern T this[int index1, int index2, int index3, int index4]
        {
            [NetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index1}, {index2}, {index3}, {index4}])")]
            get;
            [NetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index1}, {index2}, {index3}, {index4}])")]
            set;
        }

        [NetJs.Unbox(false)]
        public new extern T this[int index1, int index2, int index3, int index4, int index5]
        {
            [NetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index1}, {index2}, {index3}, {index4}, {index5}])")]
            get;
            [NetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index1}, {index2}, {index3}, {index4}, {index5}])")]
            set;
        }
    }
}
