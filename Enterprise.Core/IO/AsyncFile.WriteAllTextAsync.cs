using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.IO
{
    public static partial class AsyncFile
    {
        public static Task WriteAllTextAsync(
            string path,
            string contents)
        {
            return WriteAllTextAsync(path, contents, CancellationToken.None);
        }

        public static Task WriteAllTextAsync(
            string path,
            string contents,
            CancellationToken cancellationToken)
        {
            return WriteAllTextAsync(path, contents, DefaultEncoding, cancellationToken);
        }

        public static Task WriteAllTextAsync(
            string path,
            string contents,
            Encoding encoding)
        {
            return WriteAllTextAsync(path, contents, encoding, CancellationToken.None);
        }

        public static Task WriteAllTextAsync(
            string path,
            string contents,
            Encoding encoding,
            CancellationToken cancellationToken)
        {
            Check.NotEmpty(path, "path");
            Check.NotNull(encoding, "encoding");

            return Impl.WriteAllTextAsync(path, contents, encoding, cancellationToken);
        }
    }
}
