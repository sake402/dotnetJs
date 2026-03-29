using NetJs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public static partial class RuntimeHelpers
    {
        public static IPromise TaskToPromise(Task task)
        {
            if (Script.TypeOf(task) == "Promise")
                return task.As<IPromise>();
            return new Promise<object>((resolve, reject) =>
            {
                task.ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        resolve(t.As<Task<object>>().Result);
                    }
                    else
                    {
                        reject(t.Exception);
                    }
                });
            });
        }

        public static RefOrPointer<T?> CreateObjectRefrence<T>(Func<T> getValue, Action<T>? setValue)
        {
            //int? a = 1;
            //var s = Global.IfNotNull(a, aa=>aa.ToString());
            //return new RefOrPointer<T?>((i) => getValue(), (v, i) => Global.IfNotNull(setValue, t => t.Invoke(v)));
            return new RefOrPointer<T?>((i) => getValue(), (v, i) => setValue?.Invoke(v));
        }
        
        public static RefOrPointer<T?> CreateArrayReference<T>(T[] array, int? index = null)
        {
            var refs = new RefOrPointer<T?>((i) => array[(index ?? 0) + (i ?? 0)], (v, i) => array[(index ?? 0) + (i ?? 0)] = v);
            refs._array = array;
            return refs;
        }

        public static Span<T> StackAllocSpan<T>(int? length = null, T[]? initialValues = null)
        {
            if (length == null && initialValues == null)
                throw new InvalidOperationException("One of lenght or initializers required");
            length ??= initialValues!.Length;
            var ts = new T[length.Value];
            if (initialValues != null)
            {
                Array.Copy(initialValues, ts, length.Value);
            }
            return ts;
        }

        public static ReadOnlySpan<T> StackAllocReadOnlySpan<T>(int? length = null, T[]? initialValues = null)
        {
            if (length == null && initialValues == null)
                throw new InvalidOperationException("One of lenght or initializers required");
            length ??= initialValues!.Length;
            var ts = new T[length.Value];
            if (initialValues != null)
            {
                Array.Copy(initialValues, ts, length.Value);
            }
            return ts;
        }


        public static unsafe T* StackAllocPointer<T>(int? length = null, T[]? initialValues = null)
        {
            if (length == null && initialValues == null)
                throw new InvalidOperationException("One of lenght or initializers required");
            length ??= initialValues!.Length;
            var ts = new T[length.Value];
            if (initialValues != null)
            {
                Array.Copy(initialValues, ts, length.Value);
            }
            return Script.RefP(CreateArrayReference(ts, null))!;
        }

        static int StringToHashCode(string str)
        {
            int hash = 0;
            if (str.Length == 0)
            {
                return hash;
            }
            for (int i = 0; i < str.Length; i++)
            {
                char ch = str.NativeCharCodeAt(i);
                hash = ((hash << 5) - hash) + ch; // Simple bitwise operation
                hash = hash & hash; // Convert to 32bit integer
            }
            return hash;
        }

        [NetJs.MemberReplace(nameof(InternalGetHashCode))]
        private static int InternalGetHashCodeImpl(object? o)
        {
            return NetJs.Script.Write<int>("$.$getHashCode(o)");
        }

        [NetJs.MemberReplace(nameof(GetObjectValue))]
        public static object? GetObjectValueImpl(object? obj)
        {
            return obj;
        }

        [NetJs.MemberReplace(nameof(PrepareMethod))]
        private static unsafe void PrepareMethodImpl(IntPtr method, IntPtr* instantiations, int ninst)
        {

        }

        [NetJs.MemberReplace(nameof(GetUninitializedObjectInternal))]
        private static object GetUninitializedObjectInternalImpl(IntPtr type)
        {
            throw new NotImplementedException();
        }

        [NetJs.MemberReplace(nameof(InitializeArray))]
        private static void InitializeArrayImpl(Array array, IntPtr fldHandle)
        {

        }


        [NetJs.MemberReplace(nameof(GetSpanDataFrom))]
        private static unsafe ref byte GetSpanDataFromImpl(
            IntPtr fldHandle,
            IntPtr targetTypeHandle,
            IntPtr count)
        {
            throw new NotImplementedException();
        }

        [NetJs.MemberReplace(nameof(RunClassConstructor))]
        private static void RunClassConstructorImpl(IntPtr type)
        {

        }

        [NetJs.MemberReplace(nameof(RunModuleConstructor))]
        private static void RunModuleConstructorImpl(IntPtr module)
        {

        }

        [NetJs.MemberReplace(nameof(SufficientExecutionStack))]
        private static bool SufficientExecutionStackImpl()
        {
            return true;
        }

        [NetJs.MemberReplace(nameof(InternalBox))]
        private static object InternalBoxImpl(QCallTypeHandle type, ref byte target)
        {
            throw new NotImplementedException();
        }

        [NetJs.MemberReplace(nameof(SizeOf))]
        private static int SizeOfImpl(QCallTypeHandle handle)
        {
            throw new NotImplementedException();
        }

        [NetJs.IgnoreGeneric]
        public class Lazy<T>
        {
            bool hasValue;
            T value = default!;
            Func<T> get;

            public Lazy(Func<T> get)
            {
                this.get = get;
            }

            [NetJs.Name(Constants.LazyVariableValueName)]
            public T Value
            {
                get
                {
                    if (hasValue)
                        return value;
                    value = get();
                    hasValue = true;
                    return value;
                }
            }
        }

        [NetJs.IgnoreGeneric]
        public static Lazy<T> LazyValue<T>(Func<T> getT)
        {
            return new Lazy<T>(getT);
        }

        [NetJs.Template("{handle}._ptr.$v")]
        internal static extern RuntimeAssembly QCallAssemblyHandleToRuntimeType(this QCallAssembly handle);
        [NetJs.Template("{handle}._ptr.$v")]
        internal static extern RuntimeModule QCallModuleHandleToRuntimeType(this QCallModule handle);
        [NetJs.Template("{handle}._ptr.$v")]
        internal static extern RuntimeType QCallTypeHandleToRuntimeType(this QCallTypeHandle handle);
        [NetJs.Template("{handle}._ptr")]
        internal static extern ref T GetObjectHandleOnStack<T>(this ObjectHandleOnStack handle);
    }
}
