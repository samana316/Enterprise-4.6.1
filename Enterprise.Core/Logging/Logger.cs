using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Logging
{
    public abstract class Logger : NotifyUnhandledException
    {
        public abstract void Log(LogEntry entry);

        public abstract Task LogAsync(LogEntry entry, CancellationToken cancellationToken);

        public static Logger Create(
           ILogWriter writer)
        {
            Check.NotNull(writer, "writer");

            return new NodeLogger(writer, null);
        }

        public static Logger Create(
            ILogWriter writer,
            ILogConfiguration configuration)
        {
            Check.NotNull(writer, "writer");

            return new NodeLogger(writer, configuration);
        }

        public Logger Combine(
            Logger logger)
        {
            Check.NotNull(logger, "logger");

            return new CompositeLogger(this, logger);
        }

        private class CompositeLogger : Logger
        {
            private readonly IEnumerable<Logger> loggers;

            public CompositeLogger(
                params Logger[] loggers)
            {
                Check.NotNull(loggers, "loggers");

                this.loggers = loggers;
            }

            public override event UnhandledExceptionEventHandler UnhandledException
            {
                add { this.AddUnhandledExceptionEventHandler(value); }
                remove { this.RemoveUnhandledExceptionEventHandler(value); }
            }

            public override void Log(
                LogEntry entry)
            {
                foreach (var logger in this.loggers)
                {
                    logger.Log(entry);
                }
            }

            public override Task LogAsync(
                LogEntry entry,
                CancellationToken cancellationToken)
            {
                var tasks =
                    from logger in this.loggers
                    select logger.LogAsync(entry, cancellationToken);

                return Task.WhenAll(tasks);
            }

            private void AddUnhandledExceptionEventHandler(
                UnhandledExceptionEventHandler handler)
            {
                Parallel.ForEach(
                    this.loggers,
                    logger => logger.UnhandledException += handler);
            }

            private void RemoveUnhandledExceptionEventHandler(
                UnhandledExceptionEventHandler handler)
            {
                Parallel.ForEach(
                    this.loggers,
                    logger => logger.UnhandledException -= handler);
            }
        }

        private class NodeLogger : Logger
        {
            private readonly ILogWriter writer;

            private readonly ILogConfiguration configuration;

            public NodeLogger(
                ILogWriter writer,
                ILogConfiguration configuration)
            {
                Check.NotNull(writer, "writer");

                this.writer = writer;
                this.configuration = configuration ?? new LogAllConfiguration();
            }

            public override void Log(
                LogEntry entry)
            {
                try
                {
                    if (this.configuration.ShouldLog(entry, this.writer))
                    {
                        this.writer.Write(entry);
                    }
                }
                catch (Exception exception)
                {
                    this.OnUnhandledException(exception);
                }
            }

            public override async Task LogAsync(
                LogEntry entry,
                CancellationToken cancellationToken)
            {
                try
                {
                    if (this.configuration.ShouldLog(entry, this.writer))
                    {
                        await this.writer.WriteAsync(entry, cancellationToken);
                    }
                }
                catch (Exception exception)
                {
                    this.OnUnhandledException(exception);
                }
            }

            private class LogAllConfiguration : ILogConfiguration
            {
                public bool ShouldLog(
                    LogEntry entry,
                    ILogWriter writer)
                {
                    return true;
                }
            }
        }
    }
}
