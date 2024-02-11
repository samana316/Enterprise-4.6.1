using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using Enterprise.Core.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Logging
{
    [TestClass]
    public sealed class LoggerTest
    {
        [TestMethod]
        [TestCategory("Logging")]
        [TestCategory("Unit")]
        [Timeout(30000)]
        public async Task LogSimple()
        {
            try
            {
                var entry = new LogEntry
                {
                    Message = "My Message",
                    Source = "My Source"
                };

                var logger = Logger.Create(new TraceLogWriter());

                var task1 = logger.LogAsync(entry, CancellationToken.None);

                entry.Message = "My Message 2";
                var task2 = logger.LogAsync(entry, CancellationToken.None);

                await Task.Yield();
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        private class TraceLogWriter : ILogWriter
        {
            public void Write(
                LogEntry entry)
            {
                Trace.WriteLine(entry.Message, entry.Source);
            }

            public Task WriteAsync(
                LogEntry entry, 
                CancellationToken cancellationToken)
            {
                return Task.Run(() => this.Write(entry), cancellationToken);
            }
        }
    }
}
