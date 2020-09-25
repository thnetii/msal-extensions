using System;
using System.CommandLine.Binding;
using System.CommandLine.Hosting;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole.DeviceCodeToken
{
    public class DeviceCodeExecutor
    {
        private readonly ILogger deviceCodeLogger;
        private readonly IPublicClientApplication app;
        private readonly DeviceCodeOptions deviceCodeOptions;
        private readonly Func<DeviceCodeResult, Task> deviceCodeResultCallback;

        public DeviceCodeExecutor(
            IHttpClientFactory httpClientFactory,
            IOptions<DeviceCodeOptions> deviceCodeOptions,
            IOptions<PublicClientApplicationOptions> appOptions,
            ILoggerFactory? loggerFactory = null)
        {
            var opts = appOptions?.Value ?? throw new ArgumentNullException(nameof(appOptions));
            this.deviceCodeOptions = deviceCodeOptions?.Value ??
                throw new ArgumentNullException(nameof(deviceCodeOptions));

            loggerFactory ??= Microsoft.Extensions.Logging.Abstractions
                .NullLoggerFactory.Instance;

            string deviceCodeCategory = typeof(DeviceCodeResult).Namespace +
                "." + nameof(DeviceCodeResult.DeviceCode);
            deviceCodeLogger = loggerFactory.CreateLogger(deviceCodeCategory);

            var appBuilder = PublicClientApplicationBuilder
                .CreateWithApplicationOptions(opts)
                .WithLoggerFactory(loggerFactory);
            if (!(httpClientFactory is null))
                appBuilder.WithHttpClientFactory(httpClientFactory);
            app = appBuilder.Build();

            deviceCodeResultCallback = DeviceCodeUserInteractionCallback;
        }

        public async Task RunAsync(CancellationToken cancelToken = default)
        {
            var scopes = deviceCodeOptions.Scopes ?? Enumerable.Empty<string>();
            var flowBuilder = app
                .AcquireTokenWithDeviceCode(scopes, deviceCodeResultCallback);

            var authResult = await flowBuilder.ExecuteAsync(cancelToken)
                .ConfigureAwait(continueOnCapturedContext: false);

            _ = authResult;
        }

        private Task DeviceCodeUserInteractionCallback(DeviceCodeResult dcr)
        {
            var logger = deviceCodeLogger;
            using (BeginScope(logger, string.Join(" ", dcr.Scopes), nameof(dcr.Scopes)))
            using (BeginScope(logger, dcr.UserCode, nameof(dcr.UserCode)))
            using (BeginScope(logger, dcr.VerificationUrl, nameof(dcr.VerificationUrl)))
            using (BeginScope(logger, dcr.ExpiresOn, nameof(dcr.ExpiresOn)))
                logger.LogInformation(dcr.Message);

            return Task.CompletedTask;

            static IDisposable BeginScope(ILogger logger, object value,
                string name) =>
                logger.BeginScope($"{name}: {{{name}}}", value);
        }

        internal static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(serviceProvider =>
            {
                var definition = serviceProvider
                    .GetRequiredService<DeviceCodeDefinition>();
                var modelBinder = new ModelBinder<DeviceCodeOptions>();
                modelBinder.BindMemberFromValue(_ => _.Scopes, definition.ScopesArgument);
                return modelBinder;
            });
            services.AddOptions<DeviceCodeOptions>()
                .BindCommandLine();
        }
    }
}
