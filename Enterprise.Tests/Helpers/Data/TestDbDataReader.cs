using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Tests.Helpers.Data
{
    internal sealed class TestDbDataReader
    {
        private readonly ConnectionStringSettings settings;

        public TestDbDataReader(
            ConnectionStringSettings settings)
        {
            if (ReferenceEquals(settings, null))
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this.settings = settings;
        }

        public TestDbDataReader()
            : this(GetDefaultConnectionString())
        {
        }

        public async Task<DbDataReader> ExecuteAsync(
            string query,
            CancellationToken cancellationToken)
        {
            var provider = DbProviderFactories.GetFactory(this.settings.ProviderName);

            var connection = provider.CreateConnection();
            connection.ConnectionString = this.settings.ConnectionString;

            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = query;

            return await command.ExecuteReaderAsync(
                CommandBehavior.CloseConnection, cancellationToken);
        }

        private static ConnectionStringSettings GetDefaultConnectionString()
        {
            const string providerName = "System.Data.SqlClient";
            var provider = DbProviderFactories.GetFactory(providerName);

            var connectionStringBuilder = provider.CreateConnectionStringBuilder();
            connectionStringBuilder.Add("Data Source", @"(LocalDB)\v11.0");
            connectionStringBuilder.Add("Integrated Security", true);

            return new ConnectionStringSettings
            {
                ProviderName = providerName,
                ConnectionString = connectionStringBuilder.ConnectionString
            };
        }
    }
}
