using System;

namespace Rock.Logging
{
    public class LoggerConfiguration : ILoggerConfiguration
    {
        private int _concurrencyLevel = 3;

        public bool IsLoggingEnabled { get; set; }
        public LogLevel LoggingLevel { get; set; }

        public int ConcurrencyLevel
        {
            get { return _concurrencyLevel; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("value must be greater than zero.", "value");
                }

                _concurrencyLevel = value;
            }
        }

        public bool BlockUntilComplete { get; set; }
    }
}