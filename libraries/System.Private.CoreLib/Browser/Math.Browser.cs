namespace System
{
    public partial class Math
    {
        [dotnetJs.Template("Math.random()")]
        public static extern double Random();

        [dotnetJs.MemberReplace(nameof(Acos) + "(double)")]
        [dotnetJs.Template("Math.acos({d})")]
        public static extern double AcosImpl(double d);

        [dotnetJs.MemberReplace(nameof(Acosh) + "(double)")]
        [dotnetJs.Template("Math.acosh({d})")]
        public static extern double AcoshImpl(double d);

        [dotnetJs.MemberReplace(nameof(Asin) + "(double)")]
        [dotnetJs.Template("Math.asin({d})")]
        public static extern double AsinImpl(double d);

        [dotnetJs.MemberReplace(nameof(Asinh) + "(double)")]
        [dotnetJs.Template("Math.asinh({d})")]
        public static extern double AsinhImpl(double d);

        [dotnetJs.MemberReplace(nameof(Atan) + "(double)")]
        [dotnetJs.Template("Math.atan({d})")]
        public static extern double AtanImpl(double d);

        [dotnetJs.MemberReplace(nameof(Atan2) + "(double, double)")]
        [dotnetJs.Template("Math.atan2({y}, {x})")]
        public static extern double Atan2Impl(double y, double x);

        [dotnetJs.MemberReplace(nameof(Atanh) + "(double)")]
        [dotnetJs.Template("Math.atanh({d})")]
        public static extern double AtanhImpl(double d);

        [dotnetJs.MemberReplace(nameof(Cbrt) + "(double)")]
        [dotnetJs.Template("Math.cbrt({d})")]
        public static extern double CbrtImpl(double d);

        [dotnetJs.MemberReplace(nameof(Ceiling) + "(double)")]
        [dotnetJs.Template("Math.ceil({a})")]
        public static extern double CeilingImpl(double a);

        [dotnetJs.MemberReplace(nameof(Cos) + "(double)")]
        [dotnetJs.Template("Math.cos({d})")]
        public static extern double CosImpl(double d);

        [dotnetJs.MemberReplace(nameof(Cosh) + "(double)")]
        [dotnetJs.Template("Math.cosh({value})")]
        public static extern double CoshImpl(double value);

        [dotnetJs.MemberReplace(nameof(Exp) + "(double)")]
        [dotnetJs.Template("Math.exp({d})")]
        public static extern double ExpImpl(double d);

        [dotnetJs.MemberReplace(nameof(Floor) + "(double)")]
        [dotnetJs.Template("Math.floor({d})")]
        public static extern double FloorImpl(double d);

        [dotnetJs.MemberReplace(nameof(Log) + "(double)")]
        [dotnetJs.Template("Math.log({d})")]
        public static extern double LogImpl(double d);

        [dotnetJs.MemberReplace(nameof(Log10) + "(double)")]
        [dotnetJs.Template("Math.log10({d})")]
        public static extern double Log10Impl(double d);

        [dotnetJs.MemberReplace(nameof(Pow) + "(double, double)")]
        [dotnetJs.Template("Math.pow({x}, {y})")]
        public static extern double PowImpl(double x, double y);

        [dotnetJs.MemberReplace(nameof(Sin) + "(double)")]
        [dotnetJs.Template("Math.sin({a})")]
        public static extern double SinImpl(double a);

        [dotnetJs.MemberReplace(nameof(Sinh) + "(double)")]
        [dotnetJs.Template("Math.sinh({value})")]
        public static extern double SinhImpl(double value);

        [dotnetJs.MemberReplace(nameof(Sqrt) + "(double)")]
        [dotnetJs.Template("Math.sqrt({d})")]
        public static extern double SqrtImpl(double d);

        [dotnetJs.MemberReplace(nameof(Tan) + "(double)")]
        [dotnetJs.Template("Math.tan({a})")]
        public static extern double TanImpl(double a);

        [dotnetJs.MemberReplace(nameof(Tanh) + "(double)")]
        [dotnetJs.Template("Math.tanh({value})")]
        public static extern double TanhImpl(double value);

        [dotnetJs.MemberReplace(nameof(FusedMultiplyAdd) + "(double, double, double)")]
        [dotnetJs.Template("({x} * {y}) + {z}")]
        public static extern double FusedMultiplyAddImpl(double x, double y, double z);

        [dotnetJs.MemberReplace(nameof(Log2) + "(double)")]
        [dotnetJs.Template("Math.log2")]
        public static extern double Log2Impl(double x);

        //[dotnetJs.MemberReplace(nameof(ModF))]
        //[dotnetJs.Template("({x} * {y}) + {z}")]
        //private static extern unsafe double ModFImpl(double x, double* intptr);
    }
}
