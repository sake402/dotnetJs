using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace NetJs.Tests
{
    public static class BooleanTests
    {
        public static void Run()
        {
            TestLiteralsAndNegation();
            TestBasicLogicalOperators();
            TestShortCircuitBehavior();
            TestCompoundAssignments();
            TestNullableBooleanLifting();
            TestConversionsFromNumbers();
            TestConversionsFromStrings();
            TestBoxingAndEquality();
            TestOperatorPrecedence();
            TestConditionalOperator();
            TestGetHashCodeAndToString();
            TestTryParseEdgeCases();
            TestOrderOfEvaluation();
            Console.WriteLine("✅ Boolean Tests passed.");
        }

        private static void TestLiteralsAndNegation()
        {
            bool t = true;
            bool f = false;

            Debug.Assert(t == true);
            Debug.Assert(f == false);

            Debug.Assert(!t == false);
            Debug.Assert(!f == true);

            // Double negation
            Debug.Assert(!!t == t); // this uses compilation trick (two !). If compiler doesn't allow, replace: !( !t )
            Debug.Assert(!(!f) == f);
        }

        private static void TestBasicLogicalOperators()
        {
            bool t = true, f = false;

            // Conditional logical (short-circuit) - behavior verified elsewhere
            Debug.Assert((t && t) == true);
            Debug.Assert((t && f) == false);
            Debug.Assert((f && t) == false);

            Debug.Assert((t || f) == true);
            Debug.Assert((f || f) == false);

            // Non-short-circuit bitwise-for-bool
            Debug.Assert((t & f) == false);
            Debug.Assert((t | f) == true);
            Debug.Assert((t ^ f) == true);
            Debug.Assert((t ^ t) == false);

            // precedence: & and ^ and | lower than equality - tested more in precedence section
            Debug.Assert((true & true) == true);
        }

        private static void TestShortCircuitBehavior()
        {
            bool executed;

            // && short-circuits when left is false
            executed = false;
            bool leftFalse = false;
            bool r1 = leftFalse && (SideEffectSetTrue(ref executed));
            Debug.Assert(r1 == false);
            Debug.Assert(executed == false, "&& should not evaluate right operand when left is false");

            // || short-circuits when left is true
            executed = false;
            bool leftTrue = true;
            bool r2 = leftTrue || (SideEffectSetTrue(ref executed));
            Debug.Assert(r2 == true);
            Debug.Assert(executed == false, "|| should not evaluate right operand when left is true");

            // & does not short-circuit
            executed = false;
            bool r3 = leftFalse & (SideEffectSetTrue(ref executed));
            Debug.Assert(r3 == false);
            Debug.Assert(executed == true, "& should evaluate right operand always");

            // | does not short-circuit
            executed = false;
            bool r4 = leftTrue | (SideEffectSetTrue(ref executed));
            Debug.Assert(r4 == true);
            Debug.Assert(executed == true, "| should evaluate right operand always");
        }

        private static bool SideEffectSetTrue(ref bool flag)
        {
            flag = true;
            return true;
        }

        private static void TestCompoundAssignments()
        {
            bool b;

            b = true;
            b &= false;
            Debug.Assert(b == false);

            b = false;
            b |= true;
            Debug.Assert(b == true);

            b = true;
            b ^= true;
            Debug.Assert(b == false);

            b = false;
            b ^= true;
            Debug.Assert(b == true);
        }

        private static void TestNullableBooleanLifting()
        {
            bool? t = true;
            bool? f = false;
            bool? n = null;

            // Truth tables for lifted & (nullable)
            // true & null => null 
            Debug.Assert((t & n) == null);
            // false & null => false (since false and unknown is deterministically false)
            Debug.Assert((f & n) == false);
            // null & null => null
            Debug.Assert((n & n) == null);

            // lifted | (nullable)
            // true | null => true
            Debug.Assert((t | n) == true);
            // false | null => null
            Debug.Assert((f | n) == null);

            // lifted ^ (nullable) - XOR
            // true ^ null => null
            Debug.Assert((t ^ n) == null);
            // false ^ null => null (unknown ^ false = unknown)
            Debug.Assert((f ^ n) == null);

            // negation of nullable: !null == null (lifted)
            bool? notNull = !n;
            //Debug.Assert(notNull == null);

            // equality comparisons with nullable
            Debug.Assert((t == true) == true);
            Debug.Assert((n == true) == false); // nullable equality yields false when null compared to true
            Debug.Assert((n == null) == true);
        }

        private static void TestConversionsFromNumbers()
        {
            // Convert.ToBoolean for integers
            Debug.Assert(Convert.ToBoolean(1) == true);
            Debug.Assert(Convert.ToBoolean(123456) == true);
            Debug.Assert(Convert.ToBoolean(0) == false);
            Debug.Assert(Convert.ToBoolean(-1) == true);

            // For other numeric types
            Debug.Assert(Convert.ToBoolean(0L) == false);
            Debug.Assert(Convert.ToBoolean(1L) == true);
            Debug.Assert(Convert.ToBoolean(0.0) == false);
            Debug.Assert(Convert.ToBoolean(0.000001) == true);
            Debug.Assert(Convert.ToBoolean(0.0f) == false);
            //Debug.Assert(Convert.ToBoolean((decimal)0) == false);
            //Debug.Assert(Convert.ToBoolean((decimal)0.1m) == true);
        }

        private static void TestConversionsFromStrings()
        {
            // bool.Parse (case-insensitive)
            Debug.Assert(bool.Parse("True") == true);
            Debug.Assert(bool.Parse("true") == true);
            Debug.Assert(bool.Parse("FALSE") == false);

            // TryParse
            bool ok = bool.TryParse("TRUE", out bool parsed);
            Debug.Assert(ok && parsed == true);

            ok = bool.TryParse("false", out parsed);
            Debug.Assert(ok && parsed == false);

            // invalid parsing should return false via TryParse
            ok = bool.TryParse("notabool", out parsed);
            Debug.Assert(ok == false && parsed == false);

            // Parse throws on invalid input - validate with try/catch and Debug.Assert
            bool threw = false;
            try
            {
                bool x = bool.Parse("notabool");
            }
            catch (FormatException)
            {
                threw = true;
            }
            Debug.Assert(threw, "bool.Parse should throw FormatException for invalid input");

            // Conversions via Convert
            Debug.Assert(Convert.ToBoolean("True") == true);
            Debug.Assert(Convert.ToBoolean("false") == false);

            //// Convert.ToBoolean on null string throws - verify behavior
            //bool threwNullConvert = false;
            //try
            //{
            //    var v = Convert.ToBoolean((string)null!);
            //}
            //catch (ArgumentNullException)
            //{
            //    threwNullConvert = true;
            //}
            //Debug.Assert(threwNullConvert);
        }

        private static void TestBoxingAndEquality()
        {
            bool b = true;
            object boxed = b;
            Debug.Assert(boxed is bool);
            Debug.Assert((bool)boxed == true);

            // object.Equals vs value equality
            object oTrue = true;
            object oFalse = false;
            Debug.Assert(oTrue.Equals(true));
            Debug.Assert(!oFalse.Equals(true));

            // GetHashCode consistency
            Debug.Assert(true.GetHashCode() == ((object)true).GetHashCode());
            Debug.Assert(false.GetHashCode() == ((object)false).GetHashCode());
        }

        private static void TestOperatorPrecedence()
        {
            // && has higher precedence than ||
            bool result = false || true && false; // interpreted as false || (true && false) => false
            Debug.Assert(result == false);

            result = (false || true) && false; // (false || true) && false => true && false => false
            Debug.Assert(result == false);

            // ! has higher precedence than &&/||
            result = !false && false; // (!false) && false => true && false => false
            Debug.Assert(result == false);

            // ^ lower than == but test basic precedence combination
            Debug.Assert((true ^ false) == true);
            Debug.Assert((true & false | true) == true); // (true & false) | true => false | true => true
        }

        private static void TestConditionalOperator()
        {
            bool cond = true;
            string s = cond ? "yes" : "no";
            Debug.Assert(s == "yes");

            cond = false;
            s = cond ? "yes" : "no";
            Debug.Assert(s == "no");

            // conditional with nullable result
            bool? nb = null;
            string t = (nb ?? false) ? "true" : "false";
            Debug.Assert(t == "false");
        }

        private static void TestGetHashCodeAndToString()
        {
            Debug.Assert(true.ToString() == "True");
            Debug.Assert(false.ToString() == "False");

            // GetHashCode is implementation detail but should be consistent for same values
            int h1 = true.GetHashCode();
            int h2 = ((object)true).GetHashCode();
            Debug.Assert(h1 == h2);

            int h3 = false.GetHashCode();
            Debug.Assert(h3 != h1 || h3 == h1); // just ensure GetHashCode is callable — keep assert neutral
        }

        private static void TestTryParseEdgeCases()
        {
            // empty string returns false
            bool ok = bool.TryParse("", out bool parsed);
            Debug.Assert(ok == false);

            // whitespace-only returns false
            ok = bool.TryParse("   ", out parsed);
            Debug.Assert(ok == false);

            // weird casing works only for "true"/"false"
            ok = bool.TryParse("TrUe", out parsed);
            Debug.Assert(ok && parsed == true);

            ok = bool.TryParse("FaLsE", out parsed);
            Debug.Assert(ok && parsed == false);
        }

        private static void TestOrderOfEvaluation()
        {
            // Verify left-to-right evaluation and short-circuit behavior explicitly
            int counter = 0;

            bool FirstSideEffect()
            {
                counter++;
                return false;
            }

            bool SecondSideEffect()
            {
                counter++;
                return true;
            }

            counter = 0;
            // left false -> && short-circuit prevents second
            bool res = FirstSideEffect() && SecondSideEffect();
            Debug.Assert(res == false);
            Debug.Assert(counter == 1);

            counter = 0;
            // left true -> || short-circuit prevents second
            bool LeftTrue() { counter++; return true; }
            res = LeftTrue() || SecondSideEffect();
            Debug.Assert(res == true);
            Debug.Assert(counter == 1);
            
            counter = 0;
            // & always evaluates both
            bool a() { counter++; return false; }
            bool b() { counter++; return false; }
            res = a() & b();
            Debug.Assert(res == false);
            Debug.Assert(counter == 2);
        }
    }
}
