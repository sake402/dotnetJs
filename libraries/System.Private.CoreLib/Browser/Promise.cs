using dotnetJs;

namespace System
{
    public interface IPromise
    {
        [Name("then")]
        IPromise Then(Action<object?> continuation);
        [Name("then")]
        IPromise Then(Action<object?> continuation, Action<object?> onRejected);
        [Name("catch")]
        IPromise Catch(Action<object?> continuation);
    }

    public interface IPromise<T> : IPromise
    {
        [Name("then")]
        Promise<T> Then(Action<T> continuation);
        [Name("then")]
        Promise<T> Then(Action<T> continuation, Action<object?> onRejected);
    }

    [Name("Promise")]
    [IgnoreGeneric]
    [External]
    public class Promise
    {

    }

    [Name("Promise")]
    [IgnoreGeneric]
    [External]
    public class Promise<T> : Promise, IPromise<T>
    {
        public delegate void Resolver(Union<T, Promise<T>> value);
        public delegate void Rejector(object? readon);
        public delegate void Executor(Resolver resolve, Rejector reject);
        public extern Promise();
        public extern Promise(Executor executor);
        [Name("then")]
        public extern Promise<T> Then(Action<T> onFullfilled);
        [Name("then")]
        public extern Promise<T> Then(Action<T> onFullfilled, Action<object?> onRejected);
        [Name("catch")]
        public extern Promise<T> Catch(Action<object?> continuation);
        extern IPromise IPromise.Then(Action<object?> continuation);
        extern IPromise IPromise.Then(Action<object?> continuation, Action<object?> onRejected);
        extern IPromise IPromise.Catch(Action<object?> continuation);


        [Name("all")]
        public static extern Promise<T?> All(params IPromise[] promises);
        [Name("all")]
        public static extern Promise<TResult[]> All<TResult>(params IPromise<TResult>[] promises);
        [Name("allSettled")]
        public static extern Promise<T?> AllSettled(params IPromise[] promises);
        [Name("any")]
        public static extern Promise<T?> Any(params IPromise[] promises);
        [Name("race")]
        public static extern Promise<T?> Race(params IPromise[] promises);
        [Name("reject")]
        public static extern Promise<T?> Reject(object? reason);
        [Name("resolve")]
        public static extern Promise<T?> Resolve(object? reason);

    }
}