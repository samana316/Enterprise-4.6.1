using System;
using Enterprise.Core.Common.Formatting;
using Enterprise.Core.ObjectCreations;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Logging
{
    public sealed class LoggingSettings
    {
        public LoggingSettings(
            Logger logger,
            IFactory<object, Formatter<object>> formatterFactory = null,
            IFactory<object, Severity> severityDispatcher = null)
        {
            Check.NotNull(logger, "logger");

            this.Logger = logger;
            this.FormatterFactory = formatterFactory ?? DefaultFormatterFactory.Instance;
            this.SeverityDispatcher = severityDispatcher ?? DefaultSeverityDispatcher.Instance;
        }

        public Logger Logger { get; private set; }

        public IFactory<object, Formatter<object>> FormatterFactory { get; private set; }

        public IFactory<object, Severity> SeverityDispatcher { get; private set; }

        private class DefaultFormatterFactory : IFactory<object, Formatter<object>>
        {
            public static readonly DefaultFormatterFactory Instance = new DefaultFormatterFactory();

            private DefaultFormatterFactory()
            {
            }

            public Formatter<object> Create(
                object message)
            {
                return Formatter<object>.Default;
            }
        }

        private class DefaultSeverityDispatcher : IFactory<object, Severity>
        {
            public static readonly DefaultSeverityDispatcher Instance = new DefaultSeverityDispatcher();

            private DefaultSeverityDispatcher()
            {
            }

            public Severity Create(
                object message)
            {
                if (ReferenceEquals(message, null))
                {
                    return Severity.Warning;
                }

                if (message is string)
                {
                    return Severity.Information;
                }

                if (message is Exception)
                {
                    return Severity.Error;
                }

                return Severity.Verbose;
            }
        }
    }
}
