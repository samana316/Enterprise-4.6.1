using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.IO
{
    partial class AsyncFile
    {
        public static Task<string> ReadAllTextAsync(
            string path)
        {
            return ReadAllTextAsync(path, CancellationToken.None);
        }

        public static Task<string> ReadAllTextAsync(
            string path,
            CancellationToken cancellationToken)
        {
            return ReadAllTextAsync(path, DefaultEncoding, cancellationToken);
        }

        public static Task<string> ReadAllTextAsync(
            string path,
            Encoding encoding)
        {
            return ReadAllTextAsync(path, encoding, CancellationToken.None);
        }

        public static Task<string> ReadAllTextAsync(
            string path,
            Encoding encoding,
            CancellationToken cancellationToken)
        {
            Check.NotEmpty(path, "path");
            Check.NotNull(encoding, "encoding");

            return Impl.ReadAllTextAsync(path, encoding, cancellationToken);
        }
    }
}
