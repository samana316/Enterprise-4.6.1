using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using Enterprise.Core.Linq;

namespace Enterprise.Tests.Linq.Helpers
{
    internal sealed class DataReaderAdapter
    {
        public static IAsyncEnumerable<TResult> Create<TResult>(
            ConnectionStringSettings settings,
            string query,
            Func<IDataRecord, TResult> mapper)
        {
            return AsyncEnumerable.Create<TResult>(async (yielder, cancellationToken) =>
            {
                var provider = DbProviderFactories.GetFactory(settings.ProviderName);

                using (var connection = provider.CreateConnection())
                {
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

                                await yielder.ReturnAsync(value, cancellationToken);
                            }
                        }
                    }
                }
            });
        }
    }
}
