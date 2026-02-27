using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    [dotnetJs.StaticCallConvention]
    [dotnetJs.ExternalInterfaceImplementation(typeof(ArrayEnumerator))]
    public abstract partial class Array
    {
        [dotnetJs.InlineConst]
        public const string InterfaceImplementationName = "$implements";
        [dotnetJs.InlineConst]
        public const string ElementTypeName = "$elementType";
        [dotnetJs.InlineConst]
        public const string SizesName = "$sizes";
        [dotnetJs.InlineConst]
        public const string LowerBoundsName = "$lb";
        
        //Type ElementType
        //{
        //    get => this[ElementTypeName].As<TypePrototype>()?.Type;
        //}

        [dotnetJs.MemberReplace(nameof(Length))]
        [dotnetJs.StaticCallConvention(false)]
        public extern int IntrinsicLength
        {
            [dotnetJs.Template("{this}.length")]
            get;
        }

        [dotnetJs.MemberReplace(nameof(NativeLength))]
        [dotnetJs.StaticCallConvention(false)]
        [CLSCompliant(false)]
        public extern nuint IntrinsicNativeLength
        {
            [dotnetJs.Template("{this}.length")]
            get;
        }

        [dotnetJs.MemberReplace(nameof(LongLength))]
        [dotnetJs.StaticCallConvention(false)]
        public extern int IntrinsicLongLength
        {
            [dotnetJs.Template("{this}.length")]
            get;
        }

        [dotnetJs.MemberReplace(nameof(Rank))]
        public int IntrinsicRank
        {
            //[dotnetJs.Template("{global.}System.Array." + nameof(_GetRank) + "({this})")]
            //get;
            get
            {
                var sz = this[SizesName].As<int[]>();
                if (dotnetJs.Script.IsDefined(sz))
                    return sz.Length;
                return 1;
            }
        }

        [dotnetJs.Name(dotnetJs.Constants.IsTypeName)]
        public static bool Is(object? instance)
        {
            return dotnetJs.Script.Write<bool>("window.Array.isArray(instance)");
        }

        [dotnetJs.Unbox(false)]
        public extern object? this[int index]
        {
            [dotnetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index}])")]
            [dotnetJs.Template("{this}[{index}]", "unchecked")]
            get;
            [dotnetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index}])")]
            [dotnetJs.Template("{this}[{index}] = {value}", "unchecked")]
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

        [dotnetJs.Unbox(false)]
        public extern object? this[int index1, int index2]
        {
            [dotnetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index1}, {index2}])")]
            get;
            [dotnetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index1}, {index2}])")]
            set;
        }

        [dotnetJs.Unbox(false)]
        public extern object? this[int index1, int index2, int index3]
        {
            [dotnetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index1}, {index2}, {index3}])")]
            get;
            [dotnetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index1}, {index2}, {index3}])")]
            set;
        }

        [dotnetJs.Unbox(false)]
        public extern object? this[int index1, int index2, int index3, int index4]
        {
            [dotnetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index1}, {index2}, {index3}, {index4}])")]
            get;
            [dotnetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index1}, {index2}, {index3}, {index4}])")]
            set;
        }

        [dotnetJs.Unbox(false)]
        public extern object? this[int index1, int index2, int index3, int index4, int index5]
        {
            [dotnetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index1}, {index2}, {index3}, {index4}, {index5}])")]
            get;
            [dotnetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index1}, {index2}, {index3}, {index4}, {index5}])")]
            set;
        }

        internal static Array _Create(Type type, int[] sizes, int[]? lowerBounds, dotnetJs.Union<object, object[]>? fill, int depth)
        {
            unchecked
            {
                Array arr = dotnetJs.Script.Write<Array>("window.Array(sizes[depth])"); //make sure we dont create this Array class itself again
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
                        dotnetJs.Script.Write("arr[i] = innerArray");
                        //arr[i] = innerArray;
                    }
                }
                else
                {
                    if (dotnetJs.Script.IsDefined(fill))
                    {
                        if (dotnetJs.Script.Write<bool>("window.Array.isArray(fill)")/* Script.IsArray(fill)*/)
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
                            arr[i] = dotnetJs.Script.Write<object>("$.default(type)");
                        }
                    }
                }
                return arr;
            }
        }


        [dotnetJs.MemberReplace(nameof(InternalCreate))]
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

        [dotnetJs.MemberReplace(nameof(GetCorElementTypeOfElementTypeInternal))]
        private static CorElementType GetCorElementTypeOfElementTypeInternalImpl(ObjectHandleOnStack arr)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            var elementType = marr[ElementTypeName].As<RuntimeType>();
            return RuntimeTypeHandle.GetCorElementType(new QCallTypeHandle(ref elementType));
        }

        [dotnetJs.MemberReplace(nameof(IsValueOfElementTypeInternal))]
        private static bool IsValueOfElementTypeInternalImpl(ObjectHandleOnStack arr, ObjectHandleOnStack obj)
        {
            var array = arr.GetObjectHandleOnStack<Array>();
            var value = obj.GetObjectHandleOnStack<object>();
            var elementType = array[ElementTypeName].As<RuntimeType>();
            return RuntimeTypeHandle.IsInstanceOfType(new QCallTypeHandle(ref elementType), value);
        }

        [dotnetJs.MemberReplace(nameof(CanChangePrimitive))]
        private static bool CanChangePrimitiveImpl(ObjectHandleOnStack srcType, ObjectHandleOnStack dstType, bool reliable)
        {
            var src = srcType.GetObjectHandleOnStack<RuntimeType>();
            var dst = dstType.GetObjectHandleOnStack<RuntimeType>();
            return src.IsPrimitive && dst.IsPrimitive;
        }

        [dotnetJs.MemberReplace(nameof(FastCopy))]
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

        [dotnetJs.MemberReplace(nameof(GetLengthInternal))]
        private static int GetLengthInternalImpl(ObjectHandleOnStack arr, int dimension)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            var sizes = marr[SizesName].As<int[]>();
            return sizes[dimension];
        }

        [dotnetJs.MemberReplace(nameof(GetLowerBoundInternal))]
        private static int GetLowerBoundInternalImpl(ObjectHandleOnStack arr, int dimension)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            var bounds = marr[LowerBoundsName].As<int[]?>();
            if (bounds == null)
                return 0;
            return bounds[dimension];
        }

        // CAUTION! No bounds checking!
        [dotnetJs.MemberReplace(nameof(GetGenericValue_icall))]
        private static void GetGenericValue_icallImpl<T>(ObjectHandleOnStack self, int pos, out T value)
        {
            var marr = self.GetObjectHandleOnStack<Array>();
            value = marr[pos].As<T>();
        }

        // CAUTION! No bounds checking!
        [dotnetJs.MemberReplace(nameof(GetValueImpl))]
        private static void GetValueImplImpl(ObjectHandleOnStack arr, ObjectHandleOnStack res, int pos)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            res.GetObjectHandleOnStack<object?>() = marr[pos];
        }

        // CAUTION! No bounds checking!
        [dotnetJs.MemberReplace(nameof(SetGenericValue_icall))]
        private static void SetGenericValue_icallImpl<T>(ObjectHandleOnStack arr, int pos, ref T value)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            marr[pos] = value;
        }

        // CAUTION! No bounds checking!
        [dotnetJs.MemberReplace(nameof(SetValueImpl))]
        private static void SetValueImplImpl(ObjectHandleOnStack arr, ObjectHandleOnStack value, int pos)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            value.GetObjectHandleOnStack<object?>() = marr[pos];
        }

        [dotnetJs.MemberReplace(nameof(InitializeInternal))]
        private static void InitializeInternalImpl(ObjectHandleOnStack arr)
        {

        }

        // CAUTION! No bounds checking!
        [dotnetJs.MemberReplace(nameof(SetValueRelaxedImpl))]
        private static void SetValueRelaxedImplImpl(ObjectHandleOnStack arr, ObjectHandleOnStack value, int pos)
        {
            var marr = arr.GetObjectHandleOnStack<Array>();
            value.GetObjectHandleOnStack<object?>() = marr[pos];
        }

    }

    //Class only defined for generator use
    //This class makes indexing a typed array work
    [dotnetJs.External]
    public abstract class Array<T> : Array
    {
        [dotnetJs.Unbox(false)]
        public new extern T this[int index]
        {
            [dotnetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index}])")]
            [dotnetJs.Template("{this}[{index}]", "unchecked")]
            get;
            [dotnetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index}])")]
            [dotnetJs.Template("{this}[{index}] = {value}", "unchecked")]
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

        [dotnetJs.Unbox(false)]
        public new extern T this[int index1, int index2]
        {
            [dotnetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index1}, {index2}])")]
            get;
            [dotnetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index1}, {index2}])")]
            set;
        }

        [dotnetJs.Unbox(false)]
        public new extern T this[int index1, int index2, int index3]
        {
            [dotnetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index1}, {index2}, {index3}])")]
            get;
            [dotnetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index1}, {index2}, {index3}])")]
            set;
        }

        [dotnetJs.Unbox(false)]
        public new extern T this[int index1, int index2, int index3, int index4]
        {
            [dotnetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index1}, {index2}, {index3}, {index4}])")]
            get;
            [dotnetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index1}, {index2}, {index3}, {index4}])")]
            set;
        }

        [dotnetJs.Unbox(false)]
        public new extern T this[int index1, int index2, int index3, int index4, int index5]
        {
            [dotnetJs.Template("{global.}System.Array." + nameof(GetValue) + "({this}, [{index1}, {index2}, {index3}, {index4}, {index5}])")]
            get;
            [dotnetJs.Template("{global.}System.Array." + nameof(SetValue) + "({this}, {value}, [{index1}, {index2}, {index3}, {index4}, {index5}])")]
            set;
        }
    }
}
