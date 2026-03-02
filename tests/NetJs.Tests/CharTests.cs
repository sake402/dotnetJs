using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace dotnetJs.TranslationTest
{
    internal class CharTests
    {
        public static void Run()
        {
            // --- Basic construction & equality ---
            char a = 'A';
            char b = 'A';
            char c = 'B';

            Debug.Assert(a == b);
            Debug.Assert(a != c);
            Debug.Assert(a.Equals(b));
            Debug.Assert(!a.Equals(c));

            // --- Comparison ---
            Debug.Assert(a.CompareTo(b) == 0);
            Debug.Assert(a.CompareTo(c) < 0);
            Debug.Assert(c.CompareTo(a) > 0);

            // --- Numeric conversion ---
            int ai = a;
            Debug.Assert(ai == 65);
            char fromInt = (char)65;
            Debug.Assert(fromInt == 'A');

            // --- IsDigit / IsLetter / IsLetterOrDigit ---
            Debug.Assert(char.IsLetter('A'));
            Debug.Assert(char.IsLetter('z'));
            Debug.Assert(!char.IsLetter('3'));
            Debug.Assert(char.IsDigit('5'));
            Debug.Assert(!char.IsDigit('X'));
            Debug.Assert(char.IsLetterOrDigit('A'));
            Debug.Assert(char.IsLetterOrDigit('9'));
            Debug.Assert(!char.IsLetterOrDigit('%'));

            // --- IsUpper / IsLower ---
            Debug.Assert(char.IsUpper('A'));
            Debug.Assert(!char.IsUpper('a'));
            Debug.Assert(char.IsLower('a'));
            Debug.Assert(!char.IsLower('A'));

            // --- IsWhiteSpace ---
            Debug.Assert(char.IsWhiteSpace(' '));
            Debug.Assert(char.IsWhiteSpace('\t'));
            Debug.Assert(char.IsWhiteSpace('\n'));
            Debug.Assert(!char.IsWhiteSpace('A'));

            // --- IsPunctuation / IsSymbol / IsSeparator ---
            Debug.Assert(char.IsPunctuation('.'));
            Debug.Assert(char.IsPunctuation('!'));
            Debug.Assert(!char.IsPunctuation('A'));

            Debug.Assert(char.IsSymbol('+'));
            Debug.Assert(!char.IsSymbol('A'));

            Debug.Assert(char.IsSeparator(' '));
            Debug.Assert(!char.IsSeparator('X'));

            // --- IsControl ---
            Debug.Assert(char.IsControl('\n'));
            Debug.Assert(char.IsControl('\r'));
            Debug.Assert(!char.IsControl('a'));

            // --- IsSurrogate / High/Low surrogate ---
            char high = '\uD83D'; // surrogate pair high
            char low = '\uDE00';  // surrogate pair low
            Debug.Assert(char.IsHighSurrogate(high));
            Debug.Assert(char.IsLowSurrogate(low));
            Debug.Assert(!char.IsHighSurrogate(low));
            Debug.Assert(!char.IsLowSurrogate(high));
            Debug.Assert(char.IsSurrogatePair(high, low));

            // --- Case conversion ---
            Debug.Assert(char.ToLower('A') == 'a');
            Debug.Assert(char.ToUpper('a') == 'A');

            CultureInfo turkish = new("tr-TR");
            Debug.Assert(char.ToUpper('i', turkish) != char.ToUpperInvariant('i')); // Turkish "İ"

            // --- Parsing and TryParse ---
            Debug.Assert(char.TryParse("Z", out char parsed));
            Debug.Assert(parsed == 'Z');
            bool fail = char.TryParse("TooLong", out _);
            Debug.Assert(!fail);
            Debug.Assert(char.Parse("X") == 'X');

            // --- GetUnicodeCategory ---
            Debug.Assert(CharUnicodeInfo.GetUnicodeCategory('A') == UnicodeCategory.UppercaseLetter);
            Debug.Assert(CharUnicodeInfo.GetUnicodeCategory(' ') == UnicodeCategory.SpaceSeparator);
            Debug.Assert(CharUnicodeInfo.GetUnicodeCategory('.') == UnicodeCategory.OtherPunctuation);

            // --- GetNumericValue ---
            Debug.Assert(char.GetNumericValue('0') == 0);
            Debug.Assert(char.GetNumericValue('9') == 9);
            Debug.Assert(char.GetNumericValue('A') == -1);

            // --- Conversion ToString ---
            Debug.Assert(a.ToString() == "A");
            //Debug.Assert('💡'.ToString() == "💡");

            // --- TryGetNumericValue for various scripts ---
            Debug.Assert(char.GetNumericValue('٠') == 0); // Arabic zero
            Debug.Assert(char.GetNumericValue('۵') == 5); // Persian 5

            // --- Check surrogate behavior ---
            Debug.Assert(!char.IsSurrogate('A'));
            Debug.Assert(char.ConvertToUtf32(high, low) == 0x1F600); // 😀 face

            // --- Symbolic categories ---
            Debug.Assert(CharUnicodeInfo.GetUnicodeCategory('+') == UnicodeCategory.MathSymbol);
            Debug.Assert(CharUnicodeInfo.GetUnicodeCategory('$') == UnicodeCategory.CurrencySymbol);

            // --- Whitespace list validation ---
            char[] whites = { ' ', '\t', '\n', '\r', '\v', '\f' };
            foreach (var w in whites)
                Debug.Assert(char.IsWhiteSpace(w));

            // --- Check formatting & interpolation ---
            string s = $"{'X'}";
            Debug.Assert(s == "X");
            
            // --- Compare ignoring case (manual via ToUpper) ---
            Debug.Assert(char.ToUpper('a') == char.ToUpper('A'));

            // --- Boundary numeric tests ---
            Debug.Assert(char.MinValue == '\0');
            Debug.Assert(char.MaxValue == '\uFFFF');
            
            // --- Char.ConvertFromUtf32 ---
            string utf = char.ConvertFromUtf32(0x1F600); // 😀
            Debug.Assert(utf.Length == 2);
            Debug.Assert(char.IsSurrogatePair(utf[0], utf[1]));

            // --- Equality with ToLowerInvariant and ToUpperInvariant ---
            Debug.Assert(char.ToLowerInvariant('A') == 'a');
            Debug.Assert(char.ToUpperInvariant('a') == 'A');

            // --- Iteration test ---
            for (char ch = 'a'; ch < 'f'; ch++)
            {
                Debug.Assert(ch >= 'a' && ch < 'f');
            }

            // --- Numeric vs non-numeric ---
            Debug.Assert(char.IsDigit('7'));
            Debug.Assert(!char.IsDigit('x'));

            // --- Check combining marks ---
            Debug.Assert(CharUnicodeInfo.GetUnicodeCategory('\u0301') == UnicodeCategory.NonSpacingMark);

            // --- Check surrogate boundaries ---
            Debug.Assert(char.IsSurrogate('\uD800'));
            Debug.Assert(!char.IsSurrogate('\u0041'));

            // --- Equality & HashCode ---
            Debug.Assert('A'.GetHashCode() == 'A'.GetHashCode());
            Debug.Assert('A'.GetHashCode() != 'B'.GetHashCode());

            Console.WriteLine("✅ Char Tests passed.");
        }
    }
}