using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RockLib.Logging.LogProviders
{
    /// <summary>
    /// An implementation of the <see cref="ILogFormatter"/> interface that uses a template
    /// to format a log entry into JSON for Kubernetes.
    /// </summary>
    public class JsonMessageFormatter : ILogFormatter
    {
        /// <summary>
        /// Formats the specified log entry to a JSON.
        /// </summary>
        /// <param name="logEntry">The log entry to format.</param>
        /// <returns>The formatted log entry.</returns>
        public string Format(LogEntry logEntry)
        {
            var o = new
            {
                Level = GetLogLevelName(logEntry.Level),
                Time = logEntry.CreateTime.ToString("o"),
                Body = logEntry.Message,
            };
            return JsonConvert.SerializeObject(o, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }

        private string GetLogLevelName(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Audit:
                    return nameof(LogLevel.Audit);
                case LogLevel.Debug:
                    return nameof(LogLevel.Debug);
                case LogLevel.Error:
                    return nameof(LogLevel.Error);
                case LogLevel.Fatal:
                    return nameof(LogLevel.Fatal);
                case LogLevel.Info:
                    return nameof(LogLevel.Info);
                case LogLevel.NotSet:
                    return nameof(LogLevel.NotSet);
                case LogLevel.Warn:
                    return nameof(LogLevel.Warn);
                default:
                    return "Unknown";
            }
        }
    }
}
