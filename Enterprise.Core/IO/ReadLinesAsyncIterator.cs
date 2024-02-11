using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.IO
{
    internal sealed class ReadLinesAsyncIterator : AsyncIterator<string>
    {
        private readonly Func<StreamReader> readerFactory;

        private StreamReader reader;

        public ReadLinesAsyncIterator(
            Func<StreamReader> readerFactory)
        {
            Check.NotNull(readerFactory, "readerFactory");

            this.readerFactory = readerFactory;
        }

        public override AsyncIterator<string> Clone()
        {
            return new ReadLinesAsyncIterator(this.readerFactory);
        }

        public override bool MoveNext()
        {
            if (this.reader == null)
            {
                this.reader = this.readerFactory();
            }

            this.Current = this.reader.ReadLine();

            return this.Current != null;
        }

        public override async Task<bool> MoveNextAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (this.reader == null)
            {
                this.reader = this.readerFactory();
            }

            await Task.Delay(1);

            cancellationToken.ThrowIfCancellationRequested();
            this.Current = await this.reader.ReadLineAsync();

            return this.Current != null;
        }
    }
}
