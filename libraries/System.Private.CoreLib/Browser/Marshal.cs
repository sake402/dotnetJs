using dotnetJs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Runtime.InteropServices
{
    public static partial class Marshal
    {
        const int MarshalledPointerFlag = 0x1000000;

        static SimpleDictionary<object?> marsalTable = new SimpleDictionary<object?>();
        internal static unsafe IntPtr MarshalObject(void* value)
        {
            if (Script.TypeOf(value) == "number")
                return (IntPtr)value;
            int reference = MarshalledPointerFlag + Random.Shared.Next(1, 0x7FFFFFFF);
            while (marsalTable.ContainsKey(reference.ToString()))
            {
                reference = MarshalledPointerFlag + Random.Shared.Next(1, 0x7FFFFFFF);
            }
            marsalTable[reference.ToString()] = *(object*)value;
            return reference;
        }

        internal static IntPtr MarshalObject(object? value, IntPtr handle = 0, bool deleteOld = false)
        {
            if (Script.TypeOf(value) == "number")
                return (IntPtr)value;
            if (handle == 0)
            {
                handle = MarshalledPointerFlag + Random.Shared.Next(1, 0x7FFFFFFF);
                while (marsalTable.ContainsKey(handle.ToString()))
                {
                    handle = MarshalledPointerFlag + Random.Shared.Next(1, 0x7FFFFFFF);
                }
            }
            var key = handle.ToString();
            if (!deleteOld && marsalTable.ContainsKey(key))
            {
                throw new InvalidOperationException();
            }
            marsalTable[key] = value;
            return handle;
        }

        internal static object? MarshalObject(IntPtr value)
        {
            return marsalTable[value.ToString()];
        }

        internal static void Remove(IntPtr value)
        {
            marsalTable.Remove(value.ToString());
        }

        static int lastPInvokeError;
        [dotnetJs.MemberReplace(nameof(GetLastPInvokeError))]
        public static int GetLastPInvokeErrorImpl()
        {
            return lastPInvokeError;
        }

        /// <summary>
        /// Set the last platform invoke error on the current thread
        /// </summary>
        /// <param name="error">Error to set</param>
        [dotnetJs.MemberReplace(nameof(SetLastPInvokeError))]
        public static void SetLastPInvokeErrorImpl(int error)
        {
            lastPInvokeError = error;
        }

        [dotnetJs.MemberReplace(nameof(DestroyStructure))]
        public static void DestroyStructureImpl(IntPtr ptr, Type structuretype)
        {

        }

        [dotnetJs.MemberReplace(nameof(OffsetOf))]
        public static IntPtr OffsetOfImpl(Type t, string fieldName)
        {
            throw new NotImplementedException();
        }

        [dotnetJs.MemberReplace(nameof(StructureToPtr) + "(object, IntPtr, bool)")]
        public static void StructureToPtrImpl(object structure, IntPtr ptr, bool fDeleteOld)
        {
            MarshalObject(structure, ptr, fDeleteOld);
        }

        [dotnetJs.MemberReplace(nameof(PtrToStructureHelper))]
        private static void PtrToStructureHelperImpl(IntPtr ptr, object structure, bool allowValueClasses)
        {

        }

        [dotnetJs.MemberReplace(nameof(GetDelegateForFunctionPointerInternal) + "(QCallTypeHandle, IntPtr, ObjectHandleOnStack)")]
        private static void GetDelegateForFunctionPointerInternalImpl(QCallTypeHandle t, IntPtr ptr, ObjectHandleOnStack res)
        {
            Delegate? d = null;
            if ((ptr & MarshalledPointerFlag) != 0)
            {
                d = MarshalObject(ptr) as Delegate;
            }
            else
            {
                var methodInfo = AppDomain.GetMember(new ReflectionHandleModel { Value = (uint)ptr }) as MethodInfo;
                if (methodInfo != null)
                {
                    //new JSFunctionDelegate()
                }
            }
            res.GetObjectHandleOnStack<Delegate?>() = d;
        }

        [dotnetJs.MemberReplace(nameof(GetFunctionPointerForDelegateInternal))]
        private static IntPtr GetFunctionPointerForDelegateInternalImpl(Delegate d)
        {
            if (d.Method != null)
            {
                return (IntPtr)d.Method.As<RuntimeMethodInfo>()._model.Handle.Value;
            }
            return MarshalObject(d);
        }

        [dotnetJs.MemberReplace(nameof(PrelinkInternal))]
        private static void PrelinkInternalImpl(MethodInfo m)
        {

        }

        [dotnetJs.MemberReplace(nameof(SizeOfHelper))]
        private static int SizeOfHelperImpl(QCallTypeHandle t, bool throwIfNotMarshalable)
        {
            throw new NotImplementedException();
        }

    }
}
