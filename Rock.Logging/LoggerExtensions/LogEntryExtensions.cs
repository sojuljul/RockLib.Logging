using System.Runtime.CompilerServices;

namespace Rock.Logging
{
    // ReSharper disable ExplicitCallerInfoArgument
    public static partial class LoggerExtensions
    {
        /// <summary>
        /// Sets the value of the specified log entry's <see cref="ILogEntry.Level"/> property to
        /// <see cref="LogLevel.Debug"/> then logs the log entry.
        /// </summary>
        public static void Debug(
            this ILogger logger,
            ILogEntry logEntry,
            bool? blockUntilComplete = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0)
        {
            logEntry.Level = LogLevel.Debug;
            logger.Log(logEntry, blockUntilComplete, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Sets the value of the specified log entry's <see cref="ILogEntry.Level"/> property to
        /// <see cref="LogLevel.Info"/> then logs the log entry.
        /// </summary>
        public static void Info(
            this ILogger logger,
            ILogEntry logEntry,
            bool? blockUntilComplete = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0)
        {
            logEntry.Level = LogLevel.Info;
            logger.Log(logEntry, blockUntilComplete, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Sets the value of the specified log entry's <see cref="ILogEntry.Level"/> property to
        /// <see cref="LogLevel.Warn"/> then logs the log entry.
        /// </summary>
        public static void Warn(
            this ILogger logger,
            ILogEntry logEntry,
            bool? blockUntilComplete = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0)
        {
            logEntry.Level = LogLevel.Warn;
            logger.Log(logEntry, blockUntilComplete, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Sets the value of the specified log entry's <see cref="ILogEntry.Level"/> property to
        /// <see cref="LogLevel.Error"/> then logs the log entry.
        /// </summary>
        public static void Error(
            this ILogger logger,
            ILogEntry logEntry,
            bool? blockUntilComplete = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0)
        {
            logEntry.Level = LogLevel.Error;
            logger.Log(logEntry, blockUntilComplete, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Sets the value of the specified log entry's <see cref="ILogEntry.Level"/> property to
        /// <see cref="LogLevel.Fatal"/> then logs the log entry.
        /// </summary>
        public static void Fatal(
            this ILogger logger,
            ILogEntry logEntry,
            bool? blockUntilComplete = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0)
        {
            logEntry.Level = LogLevel.Fatal;
            logger.Log(logEntry, blockUntilComplete, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Sets the value of the specified log entry's <see cref="ILogEntry.Level"/> property to
        /// <see cref="LogLevel.Audit"/> then logs the log entry.
        /// </summary>
        public static void Audit(
            this ILogger logger,
            ILogEntry logEntry,
            bool? blockUntilComplete = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0)
        {
            logEntry.Level = LogLevel.Audit;
            logger.Log(logEntry, blockUntilComplete, callerMemberName, callerFilePath, callerLineNumber);
        }
    }
    // ReSharper restore ExplicitCallerInfoArgument
}