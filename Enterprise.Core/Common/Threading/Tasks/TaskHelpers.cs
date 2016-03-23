using System;
using System.Threading.Tasks;

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
    }
}
