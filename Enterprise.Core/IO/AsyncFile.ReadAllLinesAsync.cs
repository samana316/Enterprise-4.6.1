using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.IO
{
    partial class AsyncFile
    {
        public static Task<string[]> ReadAllLinesAsync(
            string path)
        {
            return ReadAllLinesAsync(path, CancellationToken.None);
        }

        public static Task<string[]> ReadAllLinesAsync(
            string path,
            CancellationToken cancellationToken)
        {
            return ReadAllLinesAsync(path, DefaultEncoding, cancellationToken);
        }

        public static Task<string[]> ReadAllLinesAsync(
            string path,
            Encoding encoding)
        {
            return ReadAllLinesAsync(path, encoding, CancellationToken.None);
        }

        public static Task<string[]> ReadAllLinesAsync(
            string path,
            Encoding encoding,
            CancellationToken cancellationToken)
        {
            Check.NotEmpty(path, "path");
            Check.NotNull(encoding, "encoding");

            return Impl.ReadAllLinesAsync(path, encoding, cancellationToken);
        }
    }
}
