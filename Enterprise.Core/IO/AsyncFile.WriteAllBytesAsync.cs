using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.IO
{
    partial class AsyncFile
    {
        public static Task WriteAllBytesAsync(
            string path,
            byte[] bytes)
        {
            return WriteAllBytesAsync(path, bytes, CancellationToken.None);
        }

        public static Task WriteAllBytesAsync(
            string path,
            byte[] bytes,
            CancellationToken cancellationToken)
        {
            Check.NotEmpty(path, "path");
            Check.NotNull(bytes, "bytes");

            return Impl.WriteAllBytesAsync(path, bytes, cancellationToken);
        }
    }
}
