using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public partial class MathF
    {
        [NetJs.Template("Math.random()")]
        public static extern float Random();

        [NetJs.MemberReplace(nameof(Acos) + "(float)")]
        [NetJs.Template("Math.acos({d})")]
        public static extern float AcosImpl(float d);

        [NetJs.MemberReplace(nameof(Acosh) + "(float)")]
        [NetJs.Template("Math.acosh({d})")]
        public static extern float AcoshImpl(float d);

        [NetJs.MemberReplace(nameof(Asin) + "(float)")]
        [NetJs.Template("Math.asin({d})")]
        public static extern float AsinImpl(float d);

        [NetJs.MemberReplace(nameof(Asinh) + "(float)")]
        [NetJs.Template("Math.asinh({d})")]
        public static extern float AsinhImpl(float d);

        [NetJs.MemberReplace(nameof(Atan) + "(float)")]
        [NetJs.Template("Math.atan({d})")]
        public static extern float AtanImpl(float d);

        [NetJs.MemberReplace(nameof(Atan2) + "(float, float)")]
        [NetJs.Template("Math.atan2({y}, {x})")]
        public static extern float Atan2Impl(float y, float x);

        [NetJs.MemberReplace(nameof(Atanh) + "(float)")]
        [NetJs.Template("Math.atanh({d})")]
        public static extern float AtanhImpl(float d);

        [NetJs.MemberReplace(nameof(Cbrt) + "(float)")]
        [NetJs.Template("Math.cbrt({d})")]
        public static extern float CbrtImpl(float d);

        [NetJs.MemberReplace(nameof(Ceiling) + "(float)")]
        [NetJs.Template("Math.ceil({a})")]
        public static extern float CeilingImpl(float a);

        [NetJs.MemberReplace(nameof(Cos) + "(float)")]
        [NetJs.Template("Math.cos({d})")]
        public static extern float CosImpl(float d);

        [NetJs.MemberReplace(nameof(Cosh) + "(float)")]
        [NetJs.Template("Math.cosh({value})")]
        public static extern float CoshImpl(float value);

        [NetJs.MemberReplace(nameof(Exp) + "(float)")]
        [NetJs.Template("Math.exp({d})")]
        public static extern float ExpImpl(float d);

        [NetJs.MemberReplace(nameof(Floor) + "(float)")]
        [NetJs.Template("Math.floor({d})")]
        public static extern float FloorImpl(float d);

        [NetJs.MemberReplace(nameof(Log) + "(float)")]
        [NetJs.Template("Math.log({d})")]
        public static extern float LogImpl(float d);

        [NetJs.MemberReplace(nameof(Log10) + "(float)")]
        [NetJs.Template("Math.log10({d})")]
        public static extern float Log10Impl(float d);

        [NetJs.MemberReplace(nameof(Pow) + "(float, float)")]
        [NetJs.Template("Math.pow({x}, {y})")]
        public static extern float PowImpl(float x, float y);

        [NetJs.MemberReplace(nameof(Sin) + "(float)")]
        [NetJs.Template("Math.sin({a})")]
        public static extern float SinImpl(float a);

        [NetJs.MemberReplace(nameof(Sinh) + "(float)")]
        [NetJs.Template("Math.sinh({value})")]
        public static extern float SinhImpl(float value);

        [NetJs.MemberReplace(nameof(Sqrt) + "(float)")]
        [NetJs.Template("Math.sqrt({d})")]
        public static extern float SqrtImpl(float d);

        [NetJs.MemberReplace(nameof(Tan) + "(float)")]
        [NetJs.Template("Math.tan({a})")]
        public static extern float TanImpl(float a);

        [NetJs.MemberReplace(nameof(Tanh) + "(float)")]
        [NetJs.Template("Math.tanh({value})")]
        public static extern float TanhImpl(float value);

        [NetJs.MemberReplace(nameof(FusedMultiplyAdd) + "(float, float, float)")]
        [NetJs.Template("({x} * {y}) + {z}")]
        public static extern float FusedMultiplyAddImpl(float x, float y, float z);

        [NetJs.MemberReplace(nameof(Log2) + "(float)")]
        [NetJs.Template("Math.log2")]
        public static extern float Log2Impl(float x);

        [NetJs.MemberReplace(nameof(ModF))]
        //[NetJs.Template("Math.truc(x)")]
        private static unsafe float ModFImpl(float x, float* intptr)
        {
            var dd = NetJs.Script.Write<float>("Math.ceil(x)");
            *intptr = dd;
            return dd;
        }
    }
}
