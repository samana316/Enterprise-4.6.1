using System.Text;
using Enterprise.Core.Linq;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.IO
{
    partial class AsyncFile
    {
        public static IAsyncEnumerable<string> ReadLines(
            string path)
        {
            return ReadLines(path, DefaultEncoding);
        }

        public static IAsyncEnumerable<string> ReadLines(
            string path,
            Encoding encoding)
        {
            Check.NotEmpty(path, "path");
            Check.NotNull(encoding, "encoding");

            return Impl.ReadLines(path, encoding);
        }
    }
}
