using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole
{
    public class DeviceCodeCommandExecutor : ICommandLineExecutor
    {
        private readonly ILogger deviceCodeLogger;
        private readonly IPublicClientApplication app;
        private readonly AcquireTokenOptions acquireTokenOptions;
        private readonly Func<DeviceCodeResult, Task> deviceCodeResultCallback;

        public DeviceCodeCommandExecutor(
            IHttpClientFactory httpClientFactory,
            IOptions<AcquireTokenOptions> acquireTokenOptions,
            IOptions<PublicClientApplicationOptions> appOptions,
            ILoggerFactory? loggerFactory = null)
        {
            var opts = appOptions?.Value
                ?? throw new ArgumentNullException(nameof(appOptions));

            this.acquireTokenOptions = acquireTokenOptions?.Value ??
                throw new ArgumentNullException(nameof(acquireTokenOptions));

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

        public async Task<int> RunAsync(CancellationToken cancelToken = default)
        {
            var scopes = acquireTokenOptions.Scopes ?? Enumerable.Empty<string>();
            var flowBuilder = app
                .AcquireTokenWithDeviceCode(scopes, deviceCodeResultCallback);

            var authResult = await flowBuilder.ExecuteAsync(cancelToken)
                .ConfigureAwait(continueOnCapturedContext: false);

            _ = authResult;

            return 0;
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
    }
}
