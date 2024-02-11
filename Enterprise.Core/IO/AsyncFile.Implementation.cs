using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;

namespace Enterprise.Core.IO
{
    public static partial class AsyncFile
    {
        private static readonly Implementation Impl = Implementation.Instance;

        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        private class Implementation
        {
            public static Implementation Instance = new Implementation();

            private Implementation()
            {
            }

            public async Task AppendAllLinesAsync(
                string path,
                IEnumerable<string> contents,
                Encoding encoding,
                CancellationToken cancellationToken)
            {
                using (var stream = new FileStream(path, FileMode.Append))
                {
                    await this.WriteLinesAsyncIterator(contents, encoding, cancellationToken, stream);
                }
            }

            public async Task AppendAllTextAsync(
                string path,
                string contents,
                Encoding encoding,
                CancellationToken cancellationToken)
            {
                using (var stream = new FileStream(path, FileMode.Append))
                {
                    var buffer = encoding.GetBytes(contents);
                    await stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                }
            }

            public async Task CopyAsync(
                string sourceFileName,
                string destFileName,
                bool overwrite,
                CancellationToken cancellationToken)
            {
                Stream sourceStream = null;
                Stream destStream = null;

                try
                {
                    sourceStream = File.OpenRead(sourceFileName);

                    if (!overwrite && File.Exists(destFileName))
                    {
                        destFileName = string.Format(
                            "{0}(1).{1}",
                            Path.GetFileNameWithoutExtension(destFileName),
                            Path.GetExtension(destFileName));
                    }

                    destStream = File.OpenWrite(destFileName);

                    await sourceStream.CopyToAsync(destStream, 81920, cancellationToken);
                }
                finally
                {
                    if (!ReferenceEquals(sourceStream, null))
                    {
                        sourceStream.Dispose();
                    }

                    if (!ReferenceEquals(destStream, null))
                    {
                        destStream.Dispose();
                    }
                }
            }

            public Task<byte[]> ReadAllBytesAsync(
                string path,
                CancellationToken cancellationToken)
            {
                Func<Stream> factory = () => File.OpenRead(path);
                var iterator = new ReadAsyncIterator(factory);

                return iterator.ToArrayAsync(cancellationToken);
            }

            public Task<string[]> ReadAllLinesAsync(
                string path, 
                Encoding encoding,
                CancellationToken cancellationToken)
            {
                var iterator = this.ReadLines(path, encoding);

                return iterator.ToArrayAsync(cancellationToken);
            }

            public async Task<string> ReadAllTextAsync(
                string path,
                Encoding encoding,
                CancellationToken cancellationToken)
            {
                var buffer = await this.ReadAllBytesAsync(path, cancellationToken);

                return encoding.GetString(buffer);
            }

            public IAsyncEnumerable<string> ReadLines(
                string path,
                Encoding encoding)
            {
                Func<StreamReader> factory = () => new StreamReader(File.OpenRead(path), encoding);

                return new ReadLinesAsyncIterator(factory);
            }

            public async Task WriteAllBytesAsync(
                string path, 
                byte[] bytes,
                CancellationToken cancellationToken)
            {
                using (var stream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                }
            }

            public async Task WriteAllLinesAsync(
                string path, 
                IEnumerable<string> contents, 
                Encoding encoding,
                CancellationToken cancellationToken)
            {
                using (var stream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    await this.WriteLinesAsyncIterator(contents, encoding, cancellationToken, stream);
                }
            }

            public async Task WriteAllTextAsync(
                string path, 
                string contents, 
                Encoding encoding,
                CancellationToken cancellationToken)
            {
                using (var stream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    var buffer = encoding.GetBytes(contents);
                    await stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                }
            }

            private Task WriteLinesAsyncIterator(
                IEnumerable<string> contents,
                Encoding encoding,
                CancellationToken cancellationToken,
                FileStream stream)
            {
                return contents.AsAsyncEnumerable().ForEachAsync(async content =>
                {
                    var lineBreak = encoding.GetBytes(Environment.NewLine);
                    await stream.WriteAsync(lineBreak, 0, lineBreak.Length, cancellationToken);

                    var buffer = encoding.GetBytes(content);
                    await stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                });
            }
        }
    }
}
