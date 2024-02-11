using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.IO
{
    partial class AsyncFile
    {
        public static Task WriteAllLinesAsync(
            string path,
            IEnumerable<string> contents)
        {
            return WriteAllLinesAsync(path, contents, CancellationToken.None);
        }

        public static Task WriteAllLinesAsync(
            string path,
            IEnumerable<string> contents,
            CancellationToken cancellationToken)
        {
            return WriteAllLinesAsync(path, contents, DefaultEncoding, cancellationToken);
        }

        public static Task WriteAllLinesAsync(
            string path,
            IEnumerable<string> contents,
            Encoding encoding)
        {
            return WriteAllLinesAsync(path, contents, encoding, CancellationToken.None);
        }

        public static Task WriteAllLinesAsync(
            string path,
            IEnumerable<string> contents,
            Encoding encoding,
            CancellationToken cancellationToken)
        {
            Check.NotEmpty(path, "path");
            Check.NotNull(contents, "contents");
            Check.NotNull(encoding, "encoding");

            return Impl.WriteAllLinesAsync(path, contents, encoding, cancellationToken);
        }
    }
}
