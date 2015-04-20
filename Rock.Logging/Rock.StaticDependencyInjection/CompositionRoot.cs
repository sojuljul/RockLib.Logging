using Rock.Logging.Diagnostics;
using Rock.Logging.Library;
using Rock.StaticDependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Rock.Logging.Rock.StaticDependencyInjection
{
    internal partial class CompositionRoot : CompositionRootBase
    {
        public override void Bootstrap()
        {
            TryImportMultiple<IContextProvider>(x => DefaultContextProviders.SetCurrent(x as IList<IContextProvider> ?? x.ToList()));
            TryImportFirst<ILogFormatter>(EmailLogProvider.SetDefaultLogFormatter, "EmailLogFormatter");
            TryImportFirst<ILogFormatter>(ConsoleLogProvider.SetDefaultLogFormatter, "ConsoleLogFormatter");
            TryImportFirst<ILogFormatter>(FileLogProvider.SetDefaultLogFormatter, "FileLogFormatter");
            TryImportFirst<ILogEntryFactory>(DefaultLogEntryFactory.SetCurrent);
            TryImportFirst<ILoggerFactory>(LoggerFactory.SetCurrent);
            TryImportFirst<IStepLoggerFactory>(DefaultStepLoggerFactory.SetCurrent);
            TryImportFirst<IXmlNamespaceProvider>(LogEntryExtendedProperties.SetXmlNamespace);
        }

        private void TryImportMultiple<TTargetType>(
            Action<IEnumerable<TTargetType>> importAction,
            string importName = null,
            ImportOptions options = null)
            where TTargetType : class
        {
            try
            {
                ImportMultiple(importAction, importName, options);
            }
            catch (Exception ex)
            {
                LibraryLogger.Log(ex, "Exception caught in static dependency injection.", "Rock.Logging");
            }
        }

        private void TryImportFirst<TTargetType>(
            Action<TTargetType> importAction,
            string importName = null,
            ImportOptions options = null)
            where TTargetType : class
        {
            try
            {
                ImportFirst(importAction, importName, options);
            }
            catch (Exception ex)
            {
                LibraryLogger.Log(ex, "Exception caught in static dependency injection.", "Rock.Logging");
            }
        }

        /// <summary>
        /// Gets a value indicating whether static dependency injection is enabled.
        /// </summary>
        public override bool IsEnabled
        {
            get
            {
                const string key = "Rock.StaticDependencyInjection.Enabled";
                var enabledValue = ConfigurationManager.AppSettings.Get(key) ?? "true";
                return enabledValue.ToLower() != "false";
            }
        }

        /// <summary>
        /// Return a collection of metadata objects that describe the export operations for a type.
        /// </summary>
        /// <param name="type">The type to get export metadata.</param>
        /// <returns>A collection of metadata objects that describe export operations.</returns>
        protected override IEnumerable<ExportInfo> GetExportInfos(Type type)
        {
            var attributes = Attribute.GetCustomAttributes(type, typeof(ExportAttribute));

            if (attributes.Length == 0)
            {
                return base.GetExportInfos(type);
            }

            return
                attributes.Cast<ExportAttribute>()
                .Select(attribute =>
                    new ExportInfo(type, attribute.Priority)
                    {
                        Disabled = attribute.Disabled,
                        Name = attribute.Name
                    });
        }
    }
}
