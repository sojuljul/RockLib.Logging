using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Rock.Threading;

namespace Rock.Logging
{
    public class Logger : ILogger
    {
        private readonly BlockingCollection<LogEntryWorkItem> _logEntryWorkItems = new BlockingCollection<LogEntryWorkItem>();
        private readonly BlockingCollection<LogProviderWorkItem> _logProviderWorkItems = new BlockingCollection<LogProviderWorkItem>();

        private readonly SemaphoreSlim _completionSemaphore = new SemaphoreSlim(0);
        private readonly SoftLock _startThreadsSoftLock = new SoftLock();

        private readonly ILoggerConfiguration _configuration;
        private readonly IEnumerable<ILogProvider> _logProviders;

        private readonly string _applicationId;

        private readonly ILogProvider _auditLogProvider;
        private readonly IThrottlingRuleEvaluator _throttlingRuleEvaluator;
        private readonly IEnumerable<IContextProvider> _contextProviders;

        private readonly object _disposeLocker = new object();
        private bool _isDisposed;

        public Logger(
            ILoggerConfiguration configuration,
            IEnumerable<ILogProvider> logProviders,
            IApplicationIdProvider applicationIdProvider = null,
            ILogProvider auditLogProvider = null,
            IThrottlingRuleEvaluator throttlingRuleEvaluator = null,
            IEnumerable<IContextProvider> contextProviders = null)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (logProviders == null)
            {
                throw new ArgumentNullException("logProviders");
            }

            if (configuration.ConcurrencyLevel < 1)
            {
                throw new ArgumentException("configuration.ConcurrencyLevel must be greater than zero.", "configuration");
            }

            // Be sure to fully realize lists so we get fast enumeration during logging.
            logProviders = logProviders.ToList();

            if (!logProviders.Any())
            {
                throw new ArgumentException("Must provide at least one log provider.", "logProviders");
            }

            _configuration = configuration;
            _logProviders = logProviders;

            _applicationId =
                applicationIdProvider != null
                    ? applicationIdProvider.GetApplicationId()
                    : ApplicationId.Current;

            _auditLogProvider = auditLogProvider; // NOTE: this can be null, and is expected.
            _throttlingRuleEvaluator = throttlingRuleEvaluator ?? new NullThrottlingRuleEvaluator();
            _contextProviders = (contextProviders ?? Enumerable.Empty<IContextProvider>()).ToList();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return
                _configuration.IsLoggingEnabled
                && logLevel >= _configuration.LoggingLevel
                && logLevel != LogLevel.NotSet;
        }

        public void Log(
            ILogEntry logEntry,
            bool? blockUntilComplete = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0)
        {
            var isNotAudit = logEntry.Level != LogLevel.Audit;

            if (_isDisposed || (isNotAudit && (!IsEnabled(logEntry.Level) || IsExcludedByThrottlingRule(logEntry))))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(logEntry.ApplicationId))
            {
                logEntry.ApplicationId = _applicationId;
            }

            if (logEntry.UniqueId == null)
            {
                logEntry.UniqueId = Guid.NewGuid().ToString();
            }

            // ReSharper disable ExplicitCallerInfoArgument
            logEntry.AddCallerInfo(callerMemberName, callerFilePath, callerLineNumber);
            // ReSharper restore ExplicitCallerInfoArgument

            foreach (var contextProvider in _contextProviders)
            {
                contextProvider.AddContextData(logEntry);
            }

            OnPreLog(logEntry);

            var logProviders =
                logEntry.Level == LogLevel.Audit && _auditLogProvider != null
                    ? Enumerable.Repeat(_auditLogProvider, 1)
                    : _logProviders.Where(x => logEntry.Level >= x.LoggingLevel);

            if (blockUntilComplete ?? _configuration.BlockUntilComplete)
            {
                new TaskBlocker(logProviders.Select(logProvider => GetWriteTask(logProvider, logEntry))).Wait();
            }
            else
            {
                EnsureWorkerThreadsStarted();
                _logEntryWorkItems.Add(new LogEntryWorkItem(logEntry, logProviders));
            }
        }

        private bool IsExcludedByThrottlingRule(ILogEntry logEntry)
        {
            return _throttlingRuleEvaluator != null && !_throttlingRuleEvaluator.ShouldLog(logEntry);
        }

        protected virtual void OnPreLog(ILogEntry logEntry)
        {
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                lock (_disposeLocker)
                {
                    if (!_isDisposed)
                    {
                        _isDisposed = true;

                        _logEntryWorkItems.CompleteAdding();
                        for (int i = 0; i < _configuration.ConcurrencyLevel; i++)
                        {
                            _completionSemaphore.Wait();
                        }

                        _logProviderWorkItems.CompleteAdding();
                        for (int i = 0; i < _configuration.ConcurrencyLevel; i++)
                        {
                            _completionSemaphore.Wait();
                        }
                    }
                }
            }
        }

        private static async Task GetWriteTask(ILogProvider logProvider, ILogEntry logEntry)
        {
            try
            {
                await logProvider.WriteAsync(logEntry);
            }
            catch (Exception ex)
            {
                // TODO: "Library-level" error handling (include log provider and log entry)
            }
        }

        private static bool TryGetWriteTask(ILogProvider logProvider, ILogEntry logEntry, out Task task)
        {
            try
            {
                task = logProvider.WriteAsync(logEntry);
                return true;
            }
            catch (Exception ex)
            {
                // TODO: "Library-level" error handling (include log provider and log entry)
                task = null;
                return false;
            }
        }

        private void EnsureWorkerThreadsStarted()
        {
            if (_startThreadsSoftLock.TryAcquire())
            {
                var threads = new Thread[_configuration.ConcurrencyLevel];
                for (int i = 0; i < _configuration.ConcurrencyLevel; i++)
                {
                    threads[i] = new Thread(ConsumeLogEntries) { IsBackground = true };
                    threads[i].Start();
                }

                threads = new Thread[_configuration.ConcurrencyLevel];
                for (int i = 0; i < _configuration.ConcurrencyLevel; i++)
                {
                    threads[i] = new Thread(ConsumeWorkItems) { IsBackground = true };
                    threads[i].Start();
                }
            }
        }

        private void ConsumeLogEntries()
        {
            try
            {
                foreach (var workItem in _logEntryWorkItems.GetConsumingEnumerable())
                {
                    foreach (var logProvider in workItem.LogProviders)
                    {
                        Task task;
                        if (TryGetWriteTask(logProvider, workItem.LogEntry, out task))
                        {
                            _logProviderWorkItems.Add(new LogProviderWorkItem(task, workItem.LogEntry, logProvider));
                        }
                    }
                }
            }
            finally
            {
                _completionSemaphore.Release();
            }
        }

        private async void ConsumeWorkItems()
        {
            try
            {
                foreach (var workItem in _logProviderWorkItems.GetConsumingEnumerable())
                {
                    try
                    {
                        await workItem.Task;
                    }
                    catch (Exception ex)
                    {
                        // TODO: "Library-level" error handling (include log provider and log entry)
                    }
                }
            }
            finally
            {
                _completionSemaphore.Release();
            }
        }

        private class TaskBlocker
        {
            private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
            private readonly int _count;

            public TaskBlocker(IEnumerable<Task> tasks)
            {
                foreach (var task in tasks)
                {
                    _count++;
                    task.ContinueWith(t => _semaphore.Release(), TaskScheduler.Default);
                }
            }

            public void Wait()
            {
                for (int i = 0; i < _count; i++)
                {
                    _semaphore.Wait();
                }
            }
        }

        private class LogEntryWorkItem
        {
            public readonly ILogEntry LogEntry;
            public readonly IEnumerable<ILogProvider> LogProviders;

            public LogEntryWorkItem(ILogEntry logEntry, IEnumerable<ILogProvider> logProviders)
            {
                LogEntry = logEntry;
                LogProviders = logProviders;
            }
        }

        private class LogProviderWorkItem
        {
            public readonly Task Task;
            public readonly ILogEntry LogEntry;
            public readonly ILogProvider LogProvider;

            public LogProviderWorkItem(Task task, ILogEntry logEntry, ILogProvider logProvider)
            {
                Task = task;
                LogEntry = logEntry;
                LogProvider = logProvider;
            }
        }
    }
}