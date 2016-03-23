using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.IO
{
    partial class AsyncFile
    {
        public static Task CopyAsync(
            string sourceFileName,
            string destFileName)
        {
            return CopyAsync(sourceFileName, destFileName, CancellationToken.None);
        }

        public static Task CopyAsync(
            string sourceFileName,
            string destFileName,
            CancellationToken cancellationToken)
        {
            return CopyAsync(sourceFileName, destFileName, false, cancellationToken);
        }

        public static Task CopyAsync(
            string sourceFileName,
            string destFileName,
            bool overwrite)
        {
            return CopyAsync(sourceFileName, destFileName, overwrite, CancellationToken.None);
        }

        public static Task CopyAsync(
            string sourceFileName,
            string destFileName,
            bool overwrite,
            CancellationToken cancellationToken)
        {
            Check.NotEmpty(sourceFileName, "sourceFileName");
            Check.NotEmpty(destFileName, "destFileName");

            return Impl.CopyAsync(sourceFileName, destFileName, overwrite, cancellationToken);
        }
    }
}
