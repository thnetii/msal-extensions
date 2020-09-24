using System;

using Microsoft.Extensions.Logging;

using ExtLogLevel = Microsoft.Extensions.Logging.LogLevel;
using MsalLogLevel = Microsoft.Identity.Client.LogLevel;

namespace Microsoft.Identity.Client
{
    public static class MsalLoggingExtensions
    {
        public static T WithLoggerFactory<T>(
            this AbstractApplicationBuilder<T> appBuilder,
            ILoggerFactory loggerFactory)
            where T : AbstractApplicationBuilder<T>
        {
            _ = appBuilder ?? throw new ArgumentNullException(nameof(appBuilder));
            loggerFactory ??= Microsoft.Extensions.Logging.Abstractions
                .NullLoggerFactory.Instance;

            var builderName = typeof(T).Name!;
            const string builderSuffix = "Builder";
            string categoryName = builderName.IndexOf(builderSuffix,
                StringComparison.OrdinalIgnoreCase) switch
            {
                int idx when idx < 0 => builderName,
                0 => typeof(AbstractApplicationBuilder<>).Namespace!,
                int idx => builderName.Substring(0, idx),
            };
            var logger = loggerFactory.CreateLogger(categoryName);
            var logCallback = logger.GetMsalLogCallback();

            return appBuilder.WithLogging(logCallback, MsalLogLevel.Verbose);
        }

        private static LogCallback GetMsalLogCallback(this ILogger logger)
        {
            return (level, message, containsPii) =>
            {
                using var s_lvl = logger.BeginScope($"{nameof(level)}: {{{nameof(level)}}}", level);
                using var s_pii = logger.BeginScope($"{nameof(containsPii)}: {{{nameof(containsPii)}}}", containsPii);
                var logLevel = level switch
                {
                    MsalLogLevel.Error => ExtLogLevel.Error,
                    MsalLogLevel.Warning => ExtLogLevel.Warning,
                    MsalLogLevel.Info => ExtLogLevel.Information,
                    MsalLogLevel.Verbose => ExtLogLevel.Debug,
                    MsalLogLevel msll when ((int)msll) > (int)MsalLogLevel.Verbose => ExtLogLevel.Trace,
                    _ => ExtLogLevel.None
                };

                logger.Log(logLevel, message);
            };
        }
    }
}
