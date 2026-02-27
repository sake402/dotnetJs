using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        private static unsafe partial int GetAllMountPoints(delegate* unmanaged<void*, byte*, void> onFound, void* context)
        {
            return 0;
        }
    }
}
