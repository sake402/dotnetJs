using NetJs;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Kernel32
    {
        internal static partial int ResolveLocaleName(string lpNameToResolve, char* lpLocaleName, int cchLocaleName)
        {
            return -1;
        }

        internal static unsafe partial int LCIDToLocaleName(int locale, char* pLocaleName, int cchName, uint dwFlags) => -1;

        internal static partial int LocaleNameToLCID(string lpName, uint dwFlags) => -1;

        internal static unsafe partial int LCMapStringEx(
                    string? lpLocaleName,
                    uint dwMapFlags,
                    char* lpSrcStr,
                    int cchSrc,
                    void* lpDestStr,
                    int cchDest,
                    void* lpVersionInformation,
                    void* lpReserved,
                    IntPtr sortHandle) => -1;

        internal static unsafe partial int FindNLSStringEx(
                    char* lpLocaleName,
                    uint dwFindNLSStringFlags,
                    char* lpStringSource,
                    int cchSource,
                    char* lpStringValue,
                    int cchValue,
                    int* pcchFound,
                    void* lpVersionInformation,
                    void* lpReserved,
                    IntPtr sortHandle) => -1;

        internal static unsafe partial int CompareStringEx(
                    char* lpLocaleName,
                    uint dwCmpFlags,
                    char* lpString1,
                    int cchCount1,
                    char* lpString2,
                    int cchCount2,
                    void* lpVersionInformation,
                    void* lpReserved,
                    IntPtr lParam) => -1;

        internal static unsafe partial int CompareStringOrdinal(
                    char* lpString1,
                    int cchCount1,
                    char* lpString2,
                    int cchCount2,
                    bool bIgnoreCase) => -1;

        internal static unsafe partial int FindStringOrdinal(
                    uint dwFindStringOrdinalFlags,
                    char* lpStringSource,
                    int cchSource,
                    char* lpStringValue,
                    int cchValue,
                    BOOL bIgnoreCase) => -1;

        internal static unsafe partial bool IsNLSDefinedString(
                    int Function,
                    uint dwFlags,
                    IntPtr lpVersionInformation,
                    char* lpString,
                    int cchStr) => false;

        internal static unsafe partial BOOL GetUserPreferredUILanguages(uint dwFlags, uint* pulNumLanguages, char* pwszLanguagesBuffer, uint* pcchLanguagesBuffer) => BOOL.FALSE;

        internal static unsafe partial int GetLocaleInfoEx(string lpLocaleName, uint LCType, void* lpLCData, int cchData) => -1;
        internal static unsafe partial bool EnumSystemLocalesEx(delegate* unmanaged<char*, uint, void*, BOOL> lpLocaleEnumProcEx, uint dwFlags, void* lParam, IntPtr reserved) => false;

        internal static unsafe partial bool EnumTimeFormatsEx(delegate* unmanaged<char*, void*, BOOL> lpTimeFmtEnumProcEx, string lpLocaleName, uint dwFlags, void* lParam) => false;

        internal static partial int GetCalendarInfoEx(string? lpLocaleName, uint Calendar, IntPtr lpReserved, uint CalType, IntPtr lpCalData, int cchData, out int lpValue)
        {
            lpValue = 0;
            return -1;
        }

        internal static partial int GetCalendarInfoEx(string? lpLocaleName, uint Calendar, IntPtr lpReserved, uint CalType, IntPtr lpCalData, int cchData, IntPtr lpValue) => -1;

        internal static partial int GetUserGeoID(int geoClass) => -1;

        internal static unsafe partial int GetGeoInfo(int location, int geoType, char* lpGeoData, int cchData, int LangId) => -1;

        internal static unsafe partial bool EnumCalendarInfoExEx(delegate* unmanaged<char*, uint, IntPtr, void*, BOOL> pCalInfoEnumProcExEx, string lpLocaleName, uint Calendar, string? lpReserved, uint CalType, void* lParam) => false;

        internal static unsafe partial bool GetNLSVersionEx(int function, string localeName, NlsVersionInfoEx* lpVersionInformation) => false;

    }
}
