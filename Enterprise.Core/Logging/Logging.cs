using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Formatting;
using Enterprise.Core.Common.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Logging
{
    public sealed class Logging
    {
        private readonly LoggingSettings settings;

        public Logging(
            LoggingSettings settings)
        {
            Check.NotNull(settings, "settings");

            this.settings = settings;
        }

        public void Log(
            object message,
            object source,
            Severity severity,
            Formatter<object> formatter)
        {
            var entry = this.CreateLogEntry(message, source, severity, formatter);

            if (ReferenceEquals(entry, null))
            {
                return;
            }

            this.settings.Logger.Log(entry);
        }

        public Task LogAsync(
            object message,
            object source,
            Severity severity,
            Formatter<object> formatter,
            CancellationToken cancellationToken)
        {
            var entry = this.CreateLogEntry(message, source, severity, formatter);

            if (ReferenceEquals(entry, null))
            {
                return TaskHelpers.Empty();
            }

            return this.settings.Logger.LogAsync(entry, cancellationToken);
        }

        private LogEntry CreateLogEntry(
            object message,
            object source,
            Severity severity,
            Formatter<object> formatter)
        {
            if (message == null && source == null)
            {
                return null;
            }

            if (source == null)
            {
                source = message;
            }

            var entry = new LogEntry
            {
                Severity = severity,
                Message = formatter.Format(message)
            };

            if (source is string)
            {
                entry.Source = source.ToString();
            }
            else
            {
                var sourceType = source as Type;
                entry.Source = sourceType != null ?
                    sourceType.AssemblyQualifiedName : source.GetType().AssemblyQualifiedName;
            }

            return entry;
        }

        private Severity GetDefaultSeverity(
            object message)
        {
            return this.settings.SeverityDispatcher.Create(message);
        }
    }
}
