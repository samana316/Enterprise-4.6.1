using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq.Reactive;

namespace Enterprise.Tests.Linq.Reactive.Helpers
{
    internal sealed class DataReaderObservable<TResult> : IAsyncObservable<TResult>
    {
        private readonly ConnectionStringSettings settings;

        private readonly string query;

        private readonly Func<IDataRecord, TResult> mapper;

        public DataReaderObservable(
            ConnectionStringSettings settings,
            string query, 
            Func<IDataRecord, TResult> mapper)
        {
            this.settings = settings;
            this.query = query;
            this.mapper = mapper;
        }

        public IDisposable Subscribe(
            IObserver<TResult> observer)
        {
            throw new NotSupportedException();
        }

        public async Task<IDisposable> SubscribeAsync(
            IAsyncObserver<TResult> observer, 
            CancellationToken cancellationToken)
        {
            var provider = DbProviderFactories.GetFactory(settings.ProviderName);

            var connection = provider.CreateConnection();
            connection.ConnectionString = settings.ConnectionString;
            await connection.OpenAsync(cancellationToken);

            using (var command = connection.CreateCommand())
            {
                command.CommandText = query;

                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var value = mapper(reader);

                        await observer.OnNextAsync(value, cancellationToken);
                    }
                }
            }

            observer.OnCompleted();

            return connection;
        }
    }
}
