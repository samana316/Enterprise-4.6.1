using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;

namespace Enterprise.Tests.Linq.Helpers
{
    internal sealed class StreamAdapter
    {
        public static IAsyncEnumerable<string> Create(
            Func<Stream> streamFactory)
        {
            return AsyncEnumerable.Create<string>((yielder, cancellationToken) =>
                ReadStreamAsync(streamFactory, yielder, cancellationToken));
        }

        private static async Task ReadStreamAsync(
            Func<Stream> streamFactory,
            IAsyncYielder<string> yielder,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var stream = streamFactory())
            {
                using (var reader = new StreamReader(stream))
                {
                    string line = null;

                    do
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        line = await reader.ReadLineAsync();

                        await yielder.ReturnAsync(line, cancellationToken);
                    }
                    while (line != null);
                }
            }
        }
    }
}
