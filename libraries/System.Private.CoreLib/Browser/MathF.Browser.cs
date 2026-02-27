using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public partial class MathF
    {
        [dotnetJs.Template("Math.random()")]
        public static extern float Random();

        [dotnetJs.MemberReplace(nameof(Acos) + "(float)")]
        [dotnetJs.Template("Math.acos({d})")]
        public static extern float AcosImpl(float d);

        [dotnetJs.MemberReplace(nameof(Acosh) + "(float)")]
        [dotnetJs.Template("Math.acosh({d})")]
        public static extern float AcoshImpl(float d);

        [dotnetJs.MemberReplace(nameof(Asin) + "(float)")]
        [dotnetJs.Template("Math.asin({d})")]
        public static extern float AsinImpl(float d);

        [dotnetJs.MemberReplace(nameof(Asinh) + "(float)")]
        [dotnetJs.Template("Math.asinh({d})")]
        public static extern float AsinhImpl(float d);

        [dotnetJs.MemberReplace(nameof(Atan) + "(float)")]
        [dotnetJs.Template("Math.atan({d})")]
        public static extern float AtanImpl(float d);

        [dotnetJs.MemberReplace(nameof(Atan2) + "(float, float)")]
        [dotnetJs.Template("Math.atan2({y}, {x})")]
        public static extern float Atan2Impl(float y, float x);

        [dotnetJs.MemberReplace(nameof(Atanh) + "(float)")]
        [dotnetJs.Template("Math.atanh({d})")]
        public static extern float AtanhImpl(float d);

        [dotnetJs.MemberReplace(nameof(Cbrt) + "(float)")]
        [dotnetJs.Template("Math.cbrt({d})")]
        public static extern float CbrtImpl(float d);

        [dotnetJs.MemberReplace(nameof(Ceiling) + "(float)")]
        [dotnetJs.Template("Math.ceil({a})")]
        public static extern float CeilingImpl(float a);

        [dotnetJs.MemberReplace(nameof(Cos) + "(float)")]
        [dotnetJs.Template("Math.cos({d})")]
        public static extern float CosImpl(float d);

        [dotnetJs.MemberReplace(nameof(Cosh) + "(float)")]
        [dotnetJs.Template("Math.cosh({value})")]
        public static extern float CoshImpl(float value);

        [dotnetJs.MemberReplace(nameof(Exp) + "(float)")]
        [dotnetJs.Template("Math.exp({d})")]
        public static extern float ExpImpl(float d);

        [dotnetJs.MemberReplace(nameof(Floor) + "(float)")]
        [dotnetJs.Template("Math.floor({d})")]
        public static extern float FloorImpl(float d);

        [dotnetJs.MemberReplace(nameof(Log) + "(float)")]
        [dotnetJs.Template("Math.log({d})")]
        public static extern float LogImpl(float d);

        [dotnetJs.MemberReplace(nameof(Log10) + "(float)")]
        [dotnetJs.Template("Math.log10({d})")]
        public static extern float Log10Impl(float d);

        [dotnetJs.MemberReplace(nameof(Pow) + "(float, float)")]
        [dotnetJs.Template("Math.pow({x}, {y})")]
        public static extern float PowImpl(float x, float y);

        [dotnetJs.MemberReplace(nameof(Sin) + "(float)")]
        [dotnetJs.Template("Math.sin({a})")]
        public static extern float SinImpl(float a);

        [dotnetJs.MemberReplace(nameof(Sinh) + "(float)")]
        [dotnetJs.Template("Math.sinh({value})")]
        public static extern float SinhImpl(float value);

        [dotnetJs.MemberReplace(nameof(Sqrt) + "(float)")]
        [dotnetJs.Template("Math.sqrt({d})")]
        public static extern float SqrtImpl(float d);

        [dotnetJs.MemberReplace(nameof(Tan) + "(float)")]
        [dotnetJs.Template("Math.tan({a})")]
        public static extern float TanImpl(float a);

        [dotnetJs.MemberReplace(nameof(Tanh) + "(float)")]
        [dotnetJs.Template("Math.tanh({value})")]
        public static extern float TanhImpl(float value);

        [dotnetJs.MemberReplace(nameof(FusedMultiplyAdd) + "(float, float, float)")]
        [dotnetJs.Template("({x} * {y}) + {z}")]
        public static extern float FusedMultiplyAddImpl(float x, float y, float z);

        [dotnetJs.MemberReplace(nameof(Log2) + "(float)")]
        [dotnetJs.Template("Math.log2")]
        public static extern float Log2Impl(float x);

        //[dotnetJs.MemberReplace(nameof(ModF))]
        //[dotnetJs.Template("({x} * {y}) + {z}")]
        //private static extern unsafe float ModFImpl(float x, float* intptr);
    }
}
