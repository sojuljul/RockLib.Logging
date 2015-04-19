namespace Rock.Logging
{
    public interface ILoggerConfiguration
    {
        bool IsLoggingEnabled { get; }
        LogLevel LoggingLevel { get; }
        int ConcurrencyLevel { get; }
        bool BlockUntilComplete { get; }
    }
}