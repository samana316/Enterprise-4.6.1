using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Logging
{
    public interface ILogWriter
    {
        void Write(LogEntry entry);

        Task WriteAsync(LogEntry entry, CancellationToken cancellationToken);
    }
}
