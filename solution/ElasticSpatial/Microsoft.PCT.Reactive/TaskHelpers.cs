
namespace Microsoft.PCT.Reactive
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Helper methods for dealing with asynchronous tasks, such as fan out aggregation.
    /// </summary>
    public static class TaskHelpers
    {
        /// <summary>
        /// Aggregate a set of task results.
        /// </summary>
        /// <typeparam name="T">Type of task results.</typeparam>
        /// <param name="tasks">The tasks.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The set of task results received until the cancellation occurred.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "I <3 generics.")]
        public static async Task<IList<Exceptional<T>>> AggregateAsync<T>(this IEnumerable<Task<T>> tasks, CancellationToken token)
        {
            var observable = tasks.ToObservable(token);
            return await observable.ToList();
        }

        /// <summary>
        /// Aggregate a set of task results.
        /// </summary>
        /// <typeparam name="T">Type of task results.</typeparam>
        /// <param name="tasks">The tasks.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The set of task results received until the cancellation occurred.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "I <3 generics.")]
        public static async Task<IList<Exceptional<T>>> AggregateSimpleAsync<T>(this IEnumerable<Task<T>> tasks, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (token.Register(() => tcs.SetResult(true)))
            {
                var whenAll = Task.WhenAll(tasks);
                var cancelled = tcs.Task;
                var whenAllOrCancelled = await Task.WhenAny(whenAll, cancelled);

                return tasks.Where(t => t.IsCompleted)
                    .Select(t => ToExceptional(t))
                    .ToList();
            }
        }

        /// <summary>
        /// Convert a collection of tasks and a cancellation token into an
        /// observable that aggregates the task results until the token
        /// expires.
        /// </summary>
        /// <typeparam name="T">Type of task results.</typeparam>
        /// <param name="tasks">The tasks.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>An observable of task results.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "I <3 generics.")]
        public static IObservable<Exceptional<T>> ToObservable<T>(this IEnumerable<Task<T>> tasks, CancellationToken token)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            var observables = tasks.Select(
                t => t.ToObservable()
                    .Select(value => value.ToExceptional())
                    .Catch((Exception ex) => Observable.Return(ex.ToExceptional<T>())));

            return observables.Merge().TakeUntil(token.ToObservable());
        }

        /// <summary>
        /// Create an observable from a cancellation token.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>An observable that returns when the token is cancelled.</returns>
        public static IObservable<Unit> ToObservable(this CancellationToken token)
        {
            return Observable.Create<Unit>(observer =>
                token.Register(() =>
                {
                    observer.OnNext(default(Unit));
                    observer.OnCompleted();
                }));
        }

        private static Exceptional<T> ToExceptional<T>(Task<T> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                return task.Result.ToExceptional();
            }
            else if (task.Status == TaskStatus.Faulted)
            {
                return Extract(task.Exception).ToExceptional<T>();
            }

            Debug.Assert(task.Status == TaskStatus.Canceled);
            try
            {
                // Force throwing of TaskCanceledException
                // No other way to get exception out
                task.Wait();

                // unreachable
            }
            catch (Exception ex)
            {
                var extracted = Extract(ex);
                if (extracted != ex)
                {
                    return Extract(ex).ToExceptional<T>();
                }

                // unreachable
                throw;
            }

            // unreachable
            throw new InvalidOperationException("Expected canceled task.");
        }

        private static Exception Extract(Exception exception)
        {
            var aggregate = exception as AggregateException;
            if (aggregate != null && aggregate.InnerExceptions.Count == 1)
            {
                return aggregate.InnerException;
            }

            return exception;
        }
    }
}
