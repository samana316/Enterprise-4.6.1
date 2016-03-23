using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.IO
{
    partial class AsyncFile
    {
        public static Task<byte[]> ReadAllBytesAsync(
            string path)
        {
            return ReadAllBytesAsync(path, CancellationToken.None);
        }

        public static Task<byte[]> ReadAllBytesAsync(
            string path,
            CancellationToken cancellationToken)
        {
            Check.NotEmpty(path, "path");

            return Impl.ReadAllBytesAsync(path, cancellationToken);
        }
    }
}
