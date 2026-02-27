using System.Collections.Generic;

namespace System
{
    //[dotnetJs.Convention(Member = dotnetJs.ConventionMember.Field | dotnetJs.ConventionMember.Method, Notation = dotnetJs.Notation.CamelCase)]
    //[dotnetJs.External]
    public static class ArrayExtensions
    {
        [dotnetJs.Template("{array}.includes({item})")]
        public static extern bool ArrayContains<T>(this T[] array, T item);

        [dotnetJs.Template("{array}.every({callback})")]
        public static extern bool Every<T>(this T[] array, Func<T, int, T[], bool> callback);

        [dotnetJs.Template("{array}.every({callback})")]
        public static extern bool Every<T>(this T[] array, Func<T, bool> callback);

        [dotnetJs.Template("{array}.filter({callback})")]
        public static extern T[] Filter<T>(this T[] array, Func<T, int, T[], bool> callback);

        [dotnetJs.Template("{array}.filter({callback})")]
        public static extern T[] Filter<T>(this T[] array, Func<T, bool> callback);
        [dotnetJs.Template("[...new Set({array})]")]
        public static extern T[] Unique<T>(this T[] array);

        [dotnetJs.Template("{array}.map({callback})")]
        public static extern TResult[] Map<TSource, TResult>(this TSource[] array, Func<TSource, int, TSource[], TResult> callback);

        [dotnetJs.Template("{array}.map({callback})")]
        public static extern TResult[] Map<TSource, TResult>(this TSource[] array, Func<TSource, TResult> callback);

        [dotnetJs.Template("{array}.some({callback})")]
        public static extern bool Some<T>(this T[] array, Func<T, int, T[], bool> callback);

        [dotnetJs.Template("{array}.some({callback})")]
        public static extern bool Some<T>(this T[] array, Func<T, bool> callback);

        [dotnetJs.Template("{source}.push({value})")]
        public static extern void Push<T>(this T[] source, T value);
        [dotnetJs.Template("{source}.push( ...{values})")]
        public static extern void Push<T>(this T[] source, params T[] values);

        [dotnetJs.Template("{array}.sort()")]
        public static extern void Sort<T>(this T[] array);

        [dotnetJs.Template("{array}.sort({compareCallback})")]
        public static extern void Sort<T>(this T[] array, Func<T, T, int> compareCallback);

        [dotnetJs.Template("{array}.forEach({callback})")]
        public static extern void ForEach<T>(this T[] array, Action<T, int, T[]> callback);

        [dotnetJs.Template("{array}.forEach({callback})")]
        public static extern void ForEach<T>(this T[] array, Action<T> callback);

        [dotnetJs.Template("{array}.lastIndexOf({searchString}, {fromIndex})")]
        public static extern int LastIndexOf<T>(this T[] array, string searchString, int fromIndex);

        [dotnetJs.Template("{array}.join()")]
        public static extern string Join<T>(this T[] array);

        [dotnetJs.Template("{array}.join({separator})")]
        public static extern string Join<T>(this T[] array, string separator);

        [dotnetJs.Template("{array}.pop()")]
        public static extern T Pop<T>(this T[] array);

        [dotnetJs.Template("{array}.reverse()")]
        public static extern void Reverse<T>(this T[] array);

        [dotnetJs.Template("{array}.shift()")]
        public static extern object Shift<T>(this T[] array);

        [dotnetJs.Template("{array}.slice({start})")]
        public static extern Array Slice<T>(this T[] array, int start);

        [dotnetJs.Template("{array}.slice({start}, {end})")]
        public static extern Array Slice<T>(this T[] array, int start, int end);

        [dotnetJs.Template("{array}.splice({start}, {deleteCount})")]
        public static extern Array Splice<T>(this T[] array, int start, int deleteCount);
        [dotnetJs.Template("{array}.splice({start}, {deleteCount}, {newItems})")]
        public static extern Array Splice<T>(this T[] array, int start, int deleteCount, params object[] newItems);

        [dotnetJs.Template("{array}.splice({items})")]
        public static extern void Unshift<T>(this T[] array, params T[] items);

        [dotnetJs.Template("{array1}.concat({array2})")]
        public static extern Array ArrayConcat(this Array array1, Array array2);

        public static T[] EnumerableToArray<T>(this IEnumerable<T> enumerable)
        {
            var arr = new T[0];
            foreach (var e in enumerable)
                arr.Push(e);
            return arr;
        }

        public static T ArraySingle<T>(this T[] arr)
        {
            if (arr.Length == 1)
                return arr[0];
            if (arr.Length > 1)
                throw new ArrayTypeMismatchException();
            throw new InvalidOperationException();
        }

        public static T? ArraySingleOrDefault<T>(this T[] arr)
        {
            if (arr.Length == 1)
                return arr[0];
            if (arr.Length > 1)
                throw new ArrayTypeMismatchException();
            return default(T);
        }

        public static T ArrayFirst<T>(this T[] arr)
        {
            if (arr.Length == 0)
                throw new InvalidOperationException();
            return arr[0];
        }

        public static T ArrayFirst<T>(this T[] arr, Func<T, bool> filter)
        {
            if (arr.Length == 0)
                throw new InvalidOperationException();
            return arr.Filter(filter)[0];
        }

        public static T? ArrayFirstOrDefault<T>(this T[] arr)
        {
            if (arr.Length == 0)
                return default(T);
            return arr[0];
        }

        public static T? ArrayFirstOrDefault<T>(this T[] arr, Func<T, bool> filter)
        {
            if (arr.Length == 0)
                return default(T);
            var arr2 = arr.Filter(filter);
            if (arr2.Length == 0)
                return default(T);
            return arr2[0];
        }

        public static T ArrayLast<T>(this T[] arr)
        {
            if (arr.Length == 0)
                throw new InvalidOperationException();
            return arr[arr.Length - 1];
        }

        public static T ArrayLast<T>(this T[] arr, Func<T, bool> filter)
        {
            if (arr.Length == 0)
                throw new InvalidOperationException();
            arr = arr.Filter(filter);
            return arr[arr.Length - 1];
        }

        public static T? ArrayLastOrDefault<T>(this T[] arr)
        {
            if (arr.Length == 0)
                return default(T);
            return arr[arr.Length - 1];
        }

        public static T? ArrayLastOrDefault<T>(this T[] arr, Func<T, bool> filter)
        {
            if (arr.Length == 0)
                return default(T);
            var arr2 = arr.Filter(filter);
            if (arr2.Length == 0)
                return default(T);
            return arr2[arr2.Length - 1];
        }

        public static bool ArrayAny<T>(this T[] arr)
        {
            return arr.Length > 0;
        }
    }
}
