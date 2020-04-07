using System;
using System.Threading.Tasks;
using FuncSharp;

namespace Mews.LocalizationBuilder.Extensions
{
    public static class TaskExtensions
    {
        public static Task<Result> Map<T, Result>(this Task<T> task, Func<T, Result> func)
        {
            return task.FlatMap(r => Task.FromResult(func(r)));
        }

        public static Task<TResult> FlatMap<T, TResult>(this Task<T> task, Func<T, Task<TResult>> func)
        {
            return task.ContinueWith(t => func(t.Result)).Unwrap();
        }

        public static Task<TResult> FlatMap<T, TResult>(this Option<T> option, Func<T, Task<TResult>> func, Func<Unit, TResult> defaultValue)
        {
            return option.Match(func, _ => Task.FromResult(defaultValue(Unit.Value)));
        }
    }
}