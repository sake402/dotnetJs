using System;
using System.Collections.Generic;
using System.Text;

namespace System.Threading
{
    public partial class Thread
    {
        static Thread _thread = new Thread();
        [dotnetJs.MemberReplace(nameof(StartInternal))]
        private static void StartInternalImpl(Thread runtimeThread, int stackSize)
        {
            if (runtimeThread != _thread)
                throw new PlatformNotSupportedException();
        }

        [dotnetJs.MemberReplace(nameof(GetCurrentOSThreadId))]
        private static ulong GetCurrentOSThreadIdImpl()
        {
            return 0;
        }

        [dotnetJs.MemberReplace(nameof(InitInternal))]
        private static void InitInternalImpl(Thread thread)
        {

        }

        [dotnetJs.MemberReplace(nameof(GetCurrentThread))]
        private static Thread GetCurrentThreadImpl()
        {
            return _thread;
        }

        [dotnetJs.MemberReplace(nameof(FreeInternal))]
        private void FreeInternalImpl()
        {

        }

        [dotnetJs.MemberReplace(nameof(GetState))]
        private static ThreadState GetStateImpl(Thread thread)
        {
            return ThreadState.Running;
        }

        [dotnetJs.MemberReplace(nameof(SetState))]
        private static void SetStateImpl(Thread thread, ThreadState set)
        {
            throw new PlatformNotSupportedException();
        }

        [dotnetJs.MemberReplace(nameof(ClrState))]
        private static void ClrStateImpl(Thread thread, ThreadState clr)
        {
            throw new PlatformNotSupportedException();
        }

        [dotnetJs.MemberReplace(nameof(GetName))]
        private static string GetNameImpl(Thread thread)
        {
            return "browser";
        }

        [dotnetJs.MemberReplace(nameof(SetName_icall))]
        private static unsafe void SetName_icallImpl(Thread thread, char* name, int nameLength)
        {
            throw new PlatformNotSupportedException();
        }

        [dotnetJs.MemberReplace(nameof(YieldInternal))]
        private static bool YieldInternalImpl()
        {
            throw new PlatformNotSupportedException();
        }

        [dotnetJs.MemberReplace(nameof(JoinInternal))]
        private static bool JoinInternalImpl(Thread thread, int millisecondsTimeout)
        {
            throw new PlatformNotSupportedException();
        }

        [dotnetJs.MemberReplace(nameof(InterruptInternal))]
        private static void InterruptInternalImpl(Thread thread)
        {
            throw new PlatformNotSupportedException();
        }

        [dotnetJs.MemberReplace(nameof(SetPriority))]
        private static void SetPriorityImpl(Thread thread, int priority)
        {
            throw new PlatformNotSupportedException();
        }

        [dotnetJs.MemberReplace(nameof(CurrentThreadIsFinalizerThread))]
        internal static bool CurrentThreadIsFinalizerThreadImpl()
        {
            return false;
        }

    }
}
