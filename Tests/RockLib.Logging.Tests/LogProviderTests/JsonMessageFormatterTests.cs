using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RockLib.Logging.LogProviders;
using Xunit;

namespace RockLib.Logging.Tests.LogProviderTests
{
    public class JsonMessageFormatterTests
    {
        [Fact]
        public void JsonMessageFormatter_LogLevelDebug()
        {
            var jsonFormatter = new JsonMessageFormatter();
            var logEntry = new LogEntry("testMessage", LogLevel.Debug);
            var test = jsonFormatter.Format(logEntry);
            Assert.Contains("Debug", test);
        }

        [Fact]
        public void JsonMessageFormatter_LogLevelAudit()
        {
            var jsonFormatter = new JsonMessageFormatter();
            var logEntry = new LogEntry("testMessage", LogLevel.Audit);
            var test = jsonFormatter.Format(logEntry);
            Assert.Contains("Audit", test);
        }

        [Fact]
        public void JsonMessageFormatter_LogLevelError()
        {
            var jsonFormatter = new JsonMessageFormatter();
            var logEntry = new LogEntry("testMessage", LogLevel.Error);
            var test = jsonFormatter.Format(logEntry);
            Assert.Contains("Error", test);
        }

        [Fact]
        public void JsonMessageFormatter_LogLevelFatal()
        {
            var jsonFormatter = new JsonMessageFormatter();
            var logEntry = new LogEntry("testMessage", LogLevel.Fatal);
            var test = jsonFormatter.Format(logEntry);
            Assert.Contains("Fatal", test);
        }

        [Fact]
        public void JsonMessageFormatter_LogLevelInfo()
        {
            var jsonFormatter = new JsonMessageFormatter();
            var logEntry = new LogEntry("testMessage", LogLevel.Info);
            var test = jsonFormatter.Format(logEntry);
            Assert.Contains("Info", test);
        }

        [Fact]
        public void JsonMessageFormatter_LogLevelWarn()
        {
            var jsonFormatter = new JsonMessageFormatter();
            var logEntry = new LogEntry("testMessage", LogLevel.Warn);
            var test = jsonFormatter.Format(logEntry);
            Assert.Contains("Warn", test);
        }
    }
}
