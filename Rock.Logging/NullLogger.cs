using System.Runtime.CompilerServices;

namespace Rock.Logging
{
    public class NullLogger : ILogger
    {
        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public void Log(
            ILogEntry logEntry,
            bool? blockUntilComplete = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0)
        {
        }

        public void Dispose()
        {
        }
    }
}