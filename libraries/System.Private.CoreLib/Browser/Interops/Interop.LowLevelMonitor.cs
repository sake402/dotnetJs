using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        static int[] monitors = new int[128];
        const int MONITOR_DESTROYED = 0;
        const int MONITOR_ACQUIRED = 1;
        const int MONITOR_RELEASED = 2;
        internal static partial IntPtr LowLevelMonitor_Create()
        {
            for (int i = 0; i < monitors.Length; i++)
            {
                if (monitors[i] == 0)
                {
                    return i + 1;
                }
            }
            return IntPtr.Zero;
        }

        internal static partial void LowLevelMonitor_Destroy(IntPtr monitor)
        {
            monitors[monitor - 1] = MONITOR_DESTROYED;
        }

        internal static partial void LowLevelMonitor_Acquire(IntPtr monitor)
        {
            monitors[monitor - 1] = MONITOR_ACQUIRED;
        }

        internal static partial void LowLevelMonitor_Release(IntPtr monitor)
        {
            monitors[monitor - 1] = MONITOR_RELEASED;
        }

        internal static partial void LowLevelMonitor_Wait(IntPtr monitor)
        {
            throw new PlatformNotSupportedException("Wait not supported on this platform");
        }

        internal static partial bool LowLevelMonitor_TimedWait(IntPtr monitor, int timeoutMilliseconds)
        {
            throw new PlatformNotSupportedException("Wait not supported on this platform");
        }

        internal static partial void LowLevelMonitor_Signal_Release(IntPtr monitor)
        {
            monitors[monitor - 1] = MONITOR_RELEASED;
        }
    }
}
