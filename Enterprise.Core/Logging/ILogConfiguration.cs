namespace Enterprise.Core.Logging
{
    public interface ILogConfiguration
    {
        bool ShouldLog(LogEntry entry, ILogWriter writer);
    }
}
