using System;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Common.Threading.Tasks
{
    public static class TaskHelpers
    {
        public static Task<TResult> Constant<TResult>(
            TResult result)
        {
            Func<TResult> func = () => result;

            return func.InvokeAsync(CancellationToken.None);
        }

        public static Task Empty()
        {
            return Task.FromResult<object>(null);
        }

        public static Task<TResult> ThrowAsync<TResult>(
            Exception exception)
        {
            var taskCompletionSource = new TaskCompletionSource<TResult>();
            taskCompletionSource.SetException(exception);

            return taskCompletionSource.Task;
        }

        internal static async void EndInvoke(
            IAsyncResult asyncResult)
        {
            if (!ReferenceEquals(asyncResult, null) &&
                !ReferenceEquals(asyncResult.AsyncWaitHandle, null))
            {
                await asyncResult.AsyncWaitHandle.ToTask();
            }
        }

        internal static Task InvokeAsync(
            this Action action,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Check.NotNull(action, "action");

            var taskCompletionSource = new TaskCompletionSource<object>();
            var callback = new AsyncCallback(result =>
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    action.EndInvoke(result);

                    taskCompletionSource.SetResult(null);
                }
                catch (Exception exception)
                {
                    taskCompletionSource.SetException(exception);
                }
            });

            action.BeginInvoke(callback, null);

            return taskCompletionSource.Task;
        }

        internal static Task<TResult> InvokeAsync<TResult>(
            this Func<TResult> func,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Check.NotNull(func, "func");

            var taskCompletionSource = new TaskCompletionSource<TResult>();
            var callback = new AsyncCallback(result =>
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var returnValue = func.EndInvoke(result);

                    taskCompletionSource.SetResult(returnValue);
                }
                catch (Exception exception)
                {
                    taskCompletionSource.SetException(exception);
                }
            });

            func.BeginInvoke(callback, null);

            return taskCompletionSource.Task;
        }

        internal static Task ToTask(
            this WaitHandle waitHandle)
        {
            Check.NotNull(waitHandle, "waitHandle");

            var taskCompletionSource = new TaskCompletionSource<object>();

            ThreadPool.RegisterWaitForSingleObject(
                waitObject: waitHandle,
                callBack: (o, timeout) => { taskCompletionSource.SetResult(null); },
                state: null,
                timeout: TimeSpan.MaxValue,
                executeOnlyOnce: true);

            return taskCompletionSource.Task;
        }
    }
}
