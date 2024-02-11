namespace Enterprise.Core.Logging
{
    public class LogEntry
    {
        public virtual string Source { get; set; }

        public virtual string Message { get; set; }

        public virtual Severity Severity { get; set; }
    }
}
