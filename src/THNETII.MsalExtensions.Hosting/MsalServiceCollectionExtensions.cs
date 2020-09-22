using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using MsalLogLevel = Microsoft.Identity.Client.LogLevel;

namespace THNETII.MsalExtensions.Hosting
{
    public static class MsalServiceCollectionExtensions
    {
        private static readonly string loggerCategory =
            typeof(ClientApplicationBase).Namespace!;

        public static IServiceCollection AddMsalNet(
            this IServiceCollection services, string name)
        {
            name ??= Options.DefaultName;

            services.AddHttpClient<IMsalHttpClientFactory, MsalHttpClient>(name);

            services.AddOptions<PublicClientApplicationOptions>(name);
            services.AddTransient(serviceProvider =>
            {
                var loggerFactory = serviceProvider
                    .GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger(loggerCategory);
                var logCallback = logger.GetMsalLogCallback();

                var msalHttpFactory = serviceProvider
                    .GetService<IMsalHttpClientFactory>();

                var options = serviceProvider
                    .GetService<IOptionsSnapshot<PublicClientApplicationOptions>>()?
                    .Get(name)
                    ??
                    ActivatorUtilities
                    .GetServiceOrCreateInstance<PublicClientApplicationOptions>(
                        serviceProvider)
                    ;

                var appBuilder = PublicClientApplicationBuilder
                    .CreateWithApplicationOptions(options)
                    .WithLogging(logCallback, MsalLogLevel.Verbose)
                    ;

                if (!(msalHttpFactory is null))
                    appBuilder.WithHttpClientFactory(msalHttpFactory);

                return appBuilder.Build();
            });

            services.AddOptions<ConfidentialClientApplicationOptions>(name);
            services.AddTransient(serviceProvider =>
            {
                var loggerFactory = serviceProvider
                    .GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger(loggerCategory);
                var logCallback = logger.GetMsalLogCallback();

                var msalHttpFactory = serviceProvider
                    .GetService<IMsalHttpClientFactory>();

                var options = serviceProvider
                    .GetService<IOptionsSnapshot<ConfidentialClientApplicationOptions>>()?
                    .Get(name)
                    ??
                    ActivatorUtilities
                    .GetServiceOrCreateInstance<ConfidentialClientApplicationOptions>(
                        serviceProvider)
                    ;

                var appBuilder = ConfidentialClientApplicationBuilder
                    .CreateWithApplicationOptions(options)
                    .WithLogging(logCallback, MsalLogLevel.Verbose)
                    ;

                if (!(msalHttpFactory is null))
                    appBuilder.WithHttpClientFactory(msalHttpFactory);

                return appBuilder.Build();
            });

            return services;
        }

        private static LogCallback GetMsalLogCallback(this ILogger logger)
        {
            return (msalLogLevel, message, containsPii) =>
            {
                var logLevel = msalLogLevel switch
                {
                    MsalLogLevel.Error => LogLevel.Error,
                    MsalLogLevel.Warning => LogLevel.Warning,
                    MsalLogLevel.Info => LogLevel.Information,
                    MsalLogLevel.Verbose => LogLevel.Debug,
                    MsalLogLevel msll when ((int)msll) > (int)MsalLogLevel.Verbose => LogLevel.Trace,
                    _ => LogLevel.None
                };

                logger.Log(logLevel, message);
            };
        }
    }
}
