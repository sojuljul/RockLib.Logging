namespace Rock.Logging.Diagnostics
{
    public class StepLoggerFactory : IStepLoggerFactory
    {
        public IStepLogger CreateStepLogger(
            ILogger logger,
            LogLevel logLevel,
            string message,
            bool? blockUntilComplete,
            string callerMemberName,
            string callerFilePath,
            int callerLineNumber)
        {
            return new StepLogger(logger, logLevel, message, blockUntilComplete, callerMemberName, callerFilePath, callerLineNumber);
        }
    }
}