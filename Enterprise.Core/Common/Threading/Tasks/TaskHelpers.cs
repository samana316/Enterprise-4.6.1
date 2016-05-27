using System;
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
            var taskCompletionSource = new TaskCompletionSource<TResult>();
            taskCompletionSource.SetResult(result);

            return taskCompletionSource.Task;
        }

        public static Task Empty()
        {
            return Task.FromResult<object>(null);
        }

        public static Task<TResult> Throw<TResult>(
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
