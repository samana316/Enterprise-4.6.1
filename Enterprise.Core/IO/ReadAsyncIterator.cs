using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.IO
{
    internal sealed class ReadAsyncIterator : AsyncIterator<byte>
    {
        private readonly Func<Stream> streamFactory;

        private Stream stream;

        public ReadAsyncIterator(
            Func<Stream> streamFactory)
        {
            Check.NotNull(streamFactory, "streamFactory");

            this.streamFactory = streamFactory;
        }

        public override AsyncIterator<byte> Clone()
        {
            return new ReadAsyncIterator(this.streamFactory);
        }

        public override bool MoveNext()
        {
            var buffer = this.CreateBuffer();
            var read = this.stream.Read(buffer, 0, 1);

            return this.ReadByte(buffer, read);
        }

        public override async Task<bool> MoveNextAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var buffer = this.CreateBuffer();
            var read = await this.stream.ReadAsync(buffer, 0, 1, cancellationToken);

            return this.ReadByte(buffer, read);
        }

        protected override void Dispose(
            bool disposing)
        {
            if (disposing && this.stream != null)
            {
                this.stream.Dispose();
            }

            base.Dispose(disposing);
        }

        private byte[] CreateBuffer()
        {
            if (this.stream == null)
            {
                this.stream = this.streamFactory();
            }

            return new byte[1];
        }

        private bool ReadByte(
            byte[] buffer,
            int read)
        {
            if (read > 0)
            {
                this.Current = buffer[0];
                return true;
            }

            return false;
        }
    }
}
