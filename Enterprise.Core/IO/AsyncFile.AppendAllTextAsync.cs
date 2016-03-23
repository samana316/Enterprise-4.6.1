using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.IO
{
    partial class AsyncFile
    {
        public static Task AppendAllTextAsync(
            string path,
            string contents)
        {
            return AppendAllTextAsync(path, contents, CancellationToken.None);
        }

        public static Task AppendAllTextAsync(
            string path,
            string contents,
            CancellationToken cancellationToken)
        {
            return AppendAllTextAsync(path, contents, DefaultEncoding, cancellationToken);
        }

        public static Task AppendAllTextAsync(
            string path,
            string contents,
            Encoding encoding)
        {
            return AppendAllTextAsync(path, contents, encoding, CancellationToken.None);
        }

        public static Task AppendAllTextAsync(
            string path,
            string contents,
            Encoding encoding,
            CancellationToken cancellationToken)
        {
            Check.NotEmpty(path, "path");
            Check.NotNull(encoding, "encoding");

            return Impl.AppendAllTextAsync(path, contents, encoding, cancellationToken);
        }
    }
}
