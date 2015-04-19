using System;
using System.Runtime.CompilerServices;

namespace Rock.Logging
{
    public interface ILogger : IDisposable
    {
        bool IsEnabled(LogLevel logLevel);
        
        void Log(
            ILogEntry logEntry,
            bool? blockUntilComplete = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0);
    }
}