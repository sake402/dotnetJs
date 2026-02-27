using dotnetJs;
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Threading
{
    public static partial class Monitor
    {
        [dotnetJs.MemberReplace(nameof(Enter))]
        public static void EnterImpl(object obj)
        {
        }

        [dotnetJs.MemberReplace(nameof(InternalExit))]
        private static void InternalExitImpl(object obj)
        {

        }

        [dotnetJs.MemberReplace(nameof(Monitor_pulse))]
        private static void Monitor_pulseImpl(object obj)
        {

        }

        [dotnetJs.MemberReplace(nameof(Monitor_pulse_all))]
        private static void Monitor_pulse_allImpl(object obj)
        {

        }

        [dotnetJs.MemberReplace(nameof(Monitor_wait))]
        internal static bool Monitor_waitImpl(object obj, int ms, bool allowInterruption)
        {
            return true;
        }

        [dotnetJs.MemberReplace(nameof(try_enter_with_atomic_var))]
        internal static void try_enter_with_atomic_varImpl(object obj, int millisecondsTimeout, bool allowInterruption, ref bool lockTaken)
        {

        }

        [dotnetJs.MemberReplace(nameof(Monitor_get_lock_contention_count))]
        private static long Monitor_get_lock_contention_countImpl()
        {
            return 0;
        }

    }
}
