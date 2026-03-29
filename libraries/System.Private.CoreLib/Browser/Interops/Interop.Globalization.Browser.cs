using NetJs;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Intrinsics;
using System.Text;

internal static partial class Interop
{
    internal static partial class Globalization
    {
        internal static partial int LoadICU()
        {
            return 1;
        }

        internal static partial void InitICUFunctions(IntPtr icuuc, IntPtr icuin, string version, string? suffix)
        {

        }

        internal static partial int GetICUVersion()
        {
            return 0;
        }

        static string? NormalizationFormToString(NormalizationForm form)
        {
            return form switch
            {
                NormalizationForm.FormC => "NFC",
                NormalizationForm.FormD => "NFD",
                NormalizationForm.FormKC => "NFKC",
                NormalizationForm.FormKD => "NFKD",
                _ => null,
            };
        }

        internal static unsafe partial int IsNormalized(NormalizationForm normalizationForm, char* src, int srcLen)
        {
            var span = new Span<char>(src, srcLen);
            var str = span.ToString();
            var formStr = NormalizationFormToString(normalizationForm);
            var normalized = Script.Write<string>("str.normalize(formStr)");
            return normalized == formStr ? 1 : 0;
        }

        internal static unsafe partial int NormalizeString(NormalizationForm normalizationForm, char* src, int srcLen, char* dstBuffer, int dstBufferCapacity)
        {
            var span = new Span<char>(src, srcLen);
            var str = span.ToString();
            var formStr = NormalizationFormToString(normalizationForm);
            var normalized = Script.Write<string>("str.normalize(formStr)");
            var dst = new Span<char>(dstBuffer, dstBufferCapacity);
            normalized.CopyTo(dst);
            return normalized.Length;
        }

        internal static unsafe partial bool GetLocaleName(string localeName, char* value, int valueLength)
        {
            return false;
        }

        internal static unsafe partial bool GetLocaleInfoString(string localeName, uint localeStringData, char* value, int valueLength, string? uiLocaleName = null)
        {
            return false;
        }

        internal static unsafe partial bool GetDefaultLocaleName(char* value, int valueLength)
        {
            return false;
        }

        internal static partial bool IsPredefinedLocale(string localeName)
        {
            return false;
        }

        internal static unsafe partial bool GetLocaleTimeFormat(string localeName, bool shortFormat, char* value, int valueLength)
        {
            return false;
        }

        internal static partial bool GetLocaleInfoInt(string localeName, uint localeNumberData, ref int value)
        {
            return false;
        }

        internal static partial bool GetLocaleInfoGroupingSizes(string localeName, uint localeGroupingData, ref int primaryGroupSize, ref int secondaryGroupSize)
        {
            return false;
        }

        internal static partial int GetLocales(char[]? value, int valueLength)
        {
            return 0;
        }

        internal static partial int ToAscii(uint flags, ReadOnlySpan<char> src, int srcLen, Span<char> dstBuffer, int dstBufferCapacity)
        {
            int i = 0;
            for (i = 0; i < srcLen && i < dstBufferCapacity; i++)
            {
                if (src[i] <= 127)
                    dstBuffer[i] = src[i];
                else
                    dstBuffer[i] = '\0'; //TODO: Transliterating Unicode to ASCII to limit loss of information
            }
            return i;
        }

        internal static partial int ToUnicode(uint flags, ReadOnlySpan<char> src, int srcLen, Span<char> dstBuffer, int dstBufferCapacity)
        {
            int i = 0;
            for (i = 0; i < srcLen && i < dstBufferCapacity; i++)
            {
                dstBuffer[i] = src[i];
            }
            return i;
        }

        [Template("String.fromcharCode({c}).toUpper().split('')[0]")]
        static extern char u_toupper(char c);
        [Template("String.fromcharCode({c}).toLower().split('')[0]")]
        static extern char u_tolower(char c);
        [Template("String.fromcharCode( ...{c} ).toUpper().split('')")]
        static extern char[] u_toupper(char[] c);
        [Template("String.fromcharCode( ...{c} ).toLower().split('')")]
        static extern char[] u_tolower(char[] c);

        internal static unsafe partial void ChangeCase(char* lpSrc, int cwSrcLength, char* lpDst, int cwDstLength, bool bToUpper)
        {
            var srcPointer = Script.Ref(lpSrc);
            var srcArray = srcPointer.ToArray();
            var cased = bToUpper ? u_toupper(srcArray) : u_tolower(srcArray);
            for (int i = 0; i < cwSrcLength && i < cwDstLength; i++)
            {
                lpDst[i] = cased[i];
            }
            //// Iterate through the string, decoding the next one or two UTF-16 code units
            //// into a codepoint and updating srcIdx to point to the next UTF-16 code unit
            //// to decode.  Then upper or lower case it, write dstCodepoint into lpDst at
            //// offset dstIdx, and update dstIdx.

            //// (The loop here has been manually cloned for each of the four cases, rather
            //// than having a single loop that internally branched based on bToUpper as the
            //// compiler wasn't doing that optimization, and it results in an ~15-20% perf
            //// improvement on longer strings.)

            ////bool isError = false;
            //int srcIdx = 0, dstIdx = 0;
            //char srcCodepoint, dstCodepoint;

            //if (bToUpper)
            //{
            //    while (srcIdx < cwSrcLength)
            //    {
            //        //U16_NEXT(lpSrc, srcIdx, cwSrcLength, srcCodepoint);
            //        srcCodepoint = lpSrc[srcIdx];
            //        //dstCodepoint = u_toupper(srcCodepoint);
            //        dstCodepoint = Script.Write<char>("String.fromcharCode(srcCodepoint).toUpper()");
            //        //U16_APPEND(lpDst, dstIdx, cwDstLength, dstCodepoint, isError);
            //        lpDst[dstIdx] = dstCodepoint;
            //        //assert(isError == false && srcIdx == dstIdx);
            //srcIdx++;
            //dstIdx++;
            //    }
            //}
            //else
            //{
            //    while (srcIdx < cwSrcLength)
            //    {
            //        //U16_NEXT(lpSrc, srcIdx, cwSrcLength, srcCodepoint);
            //        srcCodepoint = lpSrc[srcIdx];
            //        //dstCodepoint = u_tolower(srcCodepoint);
            //        //dstCodepoint = u_toupper(srcCodepoint);
            //        dstCodepoint = Script.Write<char>("String.fromcharCode(srcCodepoint).toLower()");
            //        //U16_APPEND(lpDst, dstIdx, cwDstLength, dstCodepoint, isError);
            //        lpDst[dstIdx] = dstCodepoint;
            //        //assert(isError == false && srcIdx == dstIdx);
            //srcIdx++;
            //dstIdx++;
            //    }
            //}
        }

        internal static unsafe partial void ChangeCaseInvariant(char* lpSrc, int cwSrcLength, char* lpDst, int cwDstLength, bool bToUpper)
        {
            // See algorithmic comment in ChangeCase.

            //bool isError = false;
            //(void)isError; // only used for assert
            int srcIdx = 0, dstIdx = 0;
            char srcCodepoint, dstCodepoint;

            if (bToUpper)
            {
                while (srcIdx < cwSrcLength)
                {
                    // On Windows with InvariantCulture, the LATIN SMALL LETTER DOTLESS I (U+0131)
                    // capitalizes to itself, whereas with ICU it capitalizes to LATIN CAPITAL LETTER I (U+0049).
                    // We special case it to match the Windows invariant behavior.
                    //U16_NEXT(lpSrc, srcIdx, cwSrcLength, srcCodepoint);
                    srcCodepoint = lpSrc[srcIdx];
                    dstCodepoint = ((srcCodepoint == (char)0x0131) ? (char)0x0131 : u_toupper(srcCodepoint));
                    //U16_APPEND(lpDst, dstIdx, cwDstLength, dstCodepoint, isError);
                    lpDst[dstIdx] = dstCodepoint;
                    //assert(isError == false && srcIdx == dstIdx);

                    srcIdx++;
                    dstIdx++;
                }
            }
            else
            {
                while (srcIdx < cwSrcLength)
                {
                    // On Windows with InvariantCulture, the LATIN CAPITAL LETTER I WITH DOT ABOVE (U+0130)
                    // lower cases to itself, whereas with ICU it lower cases to LATIN SMALL LETTER I (U+0069).
                    // We special case it to match the Windows invariant behavior.
                    //U16_NEXT(lpSrc, srcIdx, cwSrcLength, srcCodepoint);
                    srcCodepoint = lpSrc[srcIdx];
                    dstCodepoint = ((srcCodepoint == (char)0x0130) ? (char)0x0130 : u_tolower(srcCodepoint));
                    //U16_APPEND(lpDst, dstIdx, cwDstLength, dstCodepoint, isError);
                    lpDst[dstIdx] = dstCodepoint;
                    //assert(isError == false && srcIdx == dstIdx);
                    srcIdx++;
                    dstIdx++;
                }
            }
        }

        internal static unsafe partial void ChangeCaseTurkish(char* lpSrc, int cwSrcLength, char* lpDst, int cwDstLength, bool bToUpper)
        {
            // See algorithmic comment in ChangeCase.

            //bool isError = false;
            //(void)isError; // only used for assert
            int srcIdx = 0, dstIdx = 0;
            char srcCodepoint, dstCodepoint;

            if (bToUpper)
            {
                while (srcIdx < cwSrcLength)
                {
                    // In turkish casing, LATIN SMALL LETTER I (U+0069) upper cases to LATIN
                    // CAPITAL LETTER I WITH DOT ABOVE (U+0130).
                    //U16_NEXT(lpSrc, srcIdx, cwSrcLength, srcCodepoint);
                    srcCodepoint = lpSrc[srcIdx];
                    dstCodepoint = ((srcCodepoint == (char)0x0069) ? (char)0x0130 : u_toupper(srcCodepoint));
                    //U16_APPEND(lpDst, dstIdx, cwDstLength, dstCodepoint, isError);
                    lpDst[dstIdx] = dstCodepoint;
                    //assert(isError == false && srcIdx == dstIdx);

                    srcIdx++;
                    dstIdx++;
                }
            }
            else
            {
                while (srcIdx < cwSrcLength)
                {
                    // In turkish casing, LATIN CAPITAL LETTER I (U+0049) lower cases to
                    // LATIN SMALL LETTER DOTLESS I (U+0131).
                    //U16_NEXT(lpSrc, srcIdx, cwSrcLength, srcCodepoint);
                    srcCodepoint = lpSrc[srcIdx];
                    dstCodepoint = ((srcCodepoint == (char)0x0049) ? (char)0x0131 : u_tolower(srcCodepoint));
                    //U16_APPEND(lpDst, dstIdx, cwDstLength, dstCodepoint, isError);
                    lpDst[dstIdx] = dstCodepoint;
                    //assert(isError == false && srcIdx == dstIdx);

                    srcIdx++;
                    dstIdx++;
                }
            }
        }

        internal static unsafe partial void InitOrdinalCasingPage(int pageNumber, char* pTarget)
        {
            pageNumber <<= 8;
            for (int i = 0; i < 256; i++)
            {
                // Unfortunately, to ensure one-to-one simple mapping we have to call u_toupper on every character.
                // Using string casing ICU APIs cannot give such results even when using NULL locale to force root behavior.
                pTarget[i] = u_toupper((char)(pageNumber + i));
            }

            if (pageNumber == 0x0100)
            {
                // Disable Turkish I behavior on Ordinal operations
                pTarget[0x31] = (char)0x0131;  // Turkish lowercase i
                pTarget[0x7F] = (char)0x017F;  // // 017F;LATIN SMALL LETTER LONG S
            }
        }


        const string GREGORIAN_NAME = "gregorian";
        const string JAPANESE_NAME = "japanese";
        const string BUDDHIST_NAME = "buddhist";
        const string HEBREW_NAME = "hebrew";
        const string DANGI_NAME = "dangi";
        const string PERSIAN_NAME = "persian";
        const string ISLAMIC_NAME = "islamic";
        const string ISLAMIC_UMALQURA_NAME = "islamic-umalqura";
        const string ROC_NAME = "roc";

        static string GetCalendarName(CalendarId calendarId)
        {
            switch (calendarId)
            {
                case CalendarId.JAPAN:
                    return JAPANESE_NAME;
                case CalendarId.THAI:
                    return BUDDHIST_NAME;
                case CalendarId.HEBREW:
                    return HEBREW_NAME;
                case CalendarId.KOREA:
                    return DANGI_NAME;
                case CalendarId.PERSIAN:
                    return PERSIAN_NAME;
                case CalendarId.HIJRI:
                    return ISLAMIC_NAME;
                case CalendarId.UMALQURA:
                    return ISLAMIC_UMALQURA_NAME;
                case CalendarId.TAIWAN:
                    return ROC_NAME;
                case CalendarId.GREGORIAN:
                case CalendarId.GREGORIAN_US:
                case CalendarId.GREGORIAN_ARABIC:
                case CalendarId.GREGORIAN_ME_FRENCH:
                case CalendarId.GREGORIAN_XLIT_ENGLISH:
                case CalendarId.GREGORIAN_XLIT_FRENCH:
                case CalendarId.JULIAN:
                case CalendarId.LUNAR_ETO_CHN:
                case CalendarId.LUNAR_ETO_KOR:
                case CalendarId.LUNAR_ETO_ROKUYOU:
                case CalendarId.SAKA:
                // don't support the lunisolar calendars until we have a solid understanding
                // of how they map to the ICU/CLDR calendars
                case CalendarId.CHINESELUNISOLAR:
                case CalendarId.KOREANLUNISOLAR:
                case CalendarId.JAPANESELUNISOLAR:
                case CalendarId.TAIWANLUNISOLAR:
                default:
                    return GREGORIAN_NAME;
            }
        }


        static CalendarId GetCalendarId(string calendarName)
        {
            if (calendarName.Equals(GREGORIAN_NAME, StringComparison.InvariantCultureIgnoreCase))
                // TODO: what about the other gregorian types?
                return CalendarId.GREGORIAN;
            else if (calendarName.Equals(JAPANESE_NAME, StringComparison.InvariantCultureIgnoreCase))
                return CalendarId.JAPAN;
            else if (calendarName.Equals(BUDDHIST_NAME, StringComparison.InvariantCultureIgnoreCase))
                return CalendarId.THAI;
            else if (calendarName.Equals(HEBREW_NAME, StringComparison.InvariantCultureIgnoreCase))
                return CalendarId.HEBREW;
            else if (calendarName.Equals(DANGI_NAME, StringComparison.InvariantCultureIgnoreCase))
                return CalendarId.KOREA;
            else if (calendarName.Equals(PERSIAN_NAME, StringComparison.InvariantCultureIgnoreCase))
                return CalendarId.PERSIAN;
            else if (calendarName.Equals(ISLAMIC_NAME, StringComparison.InvariantCultureIgnoreCase))
                return CalendarId.HIJRI;
            else if (calendarName.Equals(ISLAMIC_UMALQURA_NAME, StringComparison.InvariantCultureIgnoreCase))
                return CalendarId.UMALQURA;
            else if (calendarName.Equals(ROC_NAME, StringComparison.InvariantCultureIgnoreCase))
                return CalendarId.TAIWAN;
            else
                return CalendarId.UNINITIALIZED_VALUE;
        }

        internal static partial int GetCalendars(string localeName, CalendarId[] calendars, int calendarsCapacity)
        {
            return 0;
        }

        internal static unsafe partial ResultCode GetCalendarInfo(string localeName, CalendarId calendarId, CalendarDataType calendarDataType, char* result, int resultCapacity)
        {
            return ResultCode.UnknownError;
        }

        // We skip the following DllImport because of 'Parsing function pointer types in signatures is not supported.' for some targeted
        // platforms (for example, WASM build).
        private static unsafe partial bool EnumCalendarInfo(IntPtr callback, string localeName, CalendarId calendarId, CalendarDataType calendarDataType, IntPtr context)
        {
            return false;
        }

        internal static partial int GetLatestJapaneseEra()
        {
            return 0;
        }

        internal static partial bool GetJapaneseEraStartDate(int era, out int startYear, out int startMonth, out int startDay)
        {
            startYear = -1;
            startMonth = -1;
            startDay = -1;
            return false;
        }


        internal static unsafe partial ResultCode GetSortHandle(string localeName, out IntPtr sortHandle)
        {
            sortHandle = IntPtr.Zero;
            return ResultCode.UnknownError;
        }

        internal static partial void CloseSortHandle(IntPtr handle)
        {

        }

        internal static unsafe partial int CompareString(IntPtr sortHandle, char* lpStr1, int cwStr1Len, char* lpStr2, int cwStr2Len, CompareOptions options)
        {
            return -1;
        }

        internal static unsafe partial int IndexOf(IntPtr sortHandle, char* target, int cwTargetLength, char* pSource, int cwSourceLength, CompareOptions options, int* matchLengthPtr)
        {
            return -1;
        }

        internal static unsafe partial int LastIndexOf(IntPtr sortHandle, char* target, int cwTargetLength, char* pSource, int cwSourceLength, CompareOptions options, int* matchLengthPtr)
        {
            return -1;
        }

        internal static unsafe partial bool StartsWith(IntPtr sortHandle, char* target, int cwTargetLength, char* source, int cwSourceLength, CompareOptions options, int* matchedLength)
        {
            return false;
        }

        internal static unsafe partial bool EndsWith(IntPtr sortHandle, char* target, int cwTargetLength, char* source, int cwSourceLength, CompareOptions options, int* matchedLength)
        {
            return false;
        }

        internal static partial bool StartsWith(IntPtr sortHandle, string target, int cwTargetLength, string source, int cwSourceLength, CompareOptions options)
        {
            return false;
        }

        internal static partial bool EndsWith(IntPtr sortHandle, string target, int cwTargetLength, string source, int cwSourceLength, CompareOptions options)
        {
            return false;
        }

        internal static unsafe partial int GetSortKey(IntPtr sortHandle, char* str, int strLength, byte* sortKey, int sortKeyLength, CompareOptions options)
        {
            return -1;
        }

        internal static partial int GetSortVersion(IntPtr sortHandle)
        {
            return -1;
        }

    }
}
