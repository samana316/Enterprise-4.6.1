using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.IO
{
    partial class AsyncFile
    {
        public static Task AppendAllLinesAsync(
            string path,
            IAsyncEnumerable<string> contents)
        {
            return AppendAllLinesAsync(path, contents, CancellationToken.None);
        }

        public static Task AppendAllLinesAsync(
            string path,
            IAsyncEnumerable<string> contents,
            CancellationToken cancellationToken)
        {
            return AppendAllLinesAsync(path, contents, DefaultEncoding, cancellationToken);
        }

        public static Task AppendAllLinesAsync(
            string path,
            IAsyncEnumerable<string> contents,
            Encoding encoding)
        {
            return AppendAllLinesAsync(path, contents, encoding, CancellationToken.None);
        }

        public static Task AppendAllLinesAsync(
            string path,
            IAsyncEnumerable<string> contents,
            Encoding encoding,
            CancellationToken cancellationToken)
        {
            Check.NotEmpty(path, "path");
            Check.NotNull(contents, "contents");
            Check.NotNull(encoding, "encoding");

            return Impl.AppendAllLinesAsync(path, contents, encoding, cancellationToken);
        }
    }
}
