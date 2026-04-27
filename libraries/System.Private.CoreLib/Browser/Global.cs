using NetJs;
using System;

[Convention(Member = ConventionMember.Field | ConventionMember.Method, Notation = Notation.CamelCase)]
[External]
[Name("$")]
public class Global
{
    //[IgnoreGeneric]
    //[Template("{0}")]
    //public static extern TIn? Nullable<TIn>(TIn? nullable) where TIn : struct;
    //[IgnoreGeneric]
    //[Template("{0}")]
    //public static extern TIn? Nullable<TIn>(TIn? nullable) where TIn : class;

    //[IgnoreGeneric]
    //[Name("$ifnn")]
    //public static extern TOut? IfNotNull<TIn, TOut>(TIn? nullable, Func<TIn, TOut?> whenNotNull) where TOut : struct;
    [IgnoreGeneric]
    [Name("$ifnn")]
    public static extern TOut? IfNotNull<TIn, TOut>(TIn? nullable, Func<TIn, TOut?> whenNotNull, TOut? ifNull = default(TOut));// where TIn : class;
    [IgnoreGeneric]
    [Name("$ifnn")]
    public static extern void IfNotNull<TIn>(TIn? nullable, System.Action<TIn> whenNotNull);
    [IgnoreGeneric]
    [Name("$ifnn")]
    public static extern void IfNotNullVoid<TIn>(TIn? nullable, System.Action<TIn> whenNotNull);
    //[IgnoreGeneric]
    //[Name("$ifnn")]
    //public static extern void IfNotNull<TIn>(TIn? nullable, System.Action<TIn> whenNotNull);
    [IgnoreGeneric]
    [Name("$exp")]
    public static extern TOut Expression<TOut>(Func<TOut> execute);

    [Template("setTimeout({0}, 1)")]
    public static extern int SetTimeout(System.Action handler, int delay);
    [Template("clearTimeout({0})")]
    public static extern void ClearTimeout(int timeoutID);
    [Template("setInterval({0}, 1)")]
    public static extern int SetInterval(System.Action handler, int delay);
    [Template("null")]
    public static extern T TypeInference<T>(T t);
    [Template("{0}")]
    public static extern T DelegateTypeInference<T>(T t) where T : Delegate;
}
