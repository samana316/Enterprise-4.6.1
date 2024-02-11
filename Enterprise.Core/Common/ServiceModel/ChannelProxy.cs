using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Common.ServiceModel
{
    public static class ChannelProxy
    {
        public static TResult Invoke<TChannel, TResult>(
           this Func<TChannel, TResult> operation)
        {
            Check.NotNull(operation, "operation");

            return Invoke(operation, CreateDefault<TChannel>);
        }

        public static TResult Invoke<TChannel, TResult>(
           this Func<TChannel, TResult> operation,
           Func<ChannelFactory<TChannel>> creator)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(creator, "creator");

            var factory = creator();

            try
            {
                var channel = factory.CreateChannel();

                try
                {
                    return operation(channel);
                }
                finally
                {
                    var disposable = channel as IDisposable;

                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
            finally
            {
                factory.CloseOrAbort();
            }
        }

        public static Task InvokeAsync<TChannel>(
            this Func<TChannel, Task> operation)
        {
            Check.NotNull(operation, "operation");

            return InvokeAsync(operation, CreateDefault<TChannel>);
        }

        public static Task InvokeAsync<TChannel>(
            this Func<TChannel, Task> operation,
            Func<ChannelFactory<TChannel>> creator)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(creator, "creator");

            Func<TChannel, Task<object>> overload = async channel =>
            {
                await operation(channel);

                return null;
            };

            return overload.InvokeAsync(creator);
        }

        public static Task<TResult> InvokeAsync<TChannel, TResult>(
            this Func<TChannel, Task<TResult>> operation)
        {
            Check.NotNull(operation, "operation");

            return InvokeAsync(operation, CreateDefault<TChannel>);
        }

        public static async Task<TResult> InvokeAsync<TChannel, TResult>(
            this Func<TChannel, Task<TResult>> operation,
            Func<ChannelFactory<TChannel>> creator)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(creator, "creator");

            var factory = creator();

            try
            {
                var channel = factory.CreateChannel();

                try
                {
                    return await operation(channel);
                }
                finally
                {
                    var disposable = channel as IDisposable;

                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
            finally
            {
                factory.CloseOrAbort();
            }
        }

        private static ChannelFactory<TChannel> CreateDefault<TChannel>()
        {
            return new ChannelFactory<TChannel>("*");
        }
    }
}
