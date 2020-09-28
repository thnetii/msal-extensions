using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public class DeviceCodeCommandExecutor
        : PublicClientApplicationAcquireTokenExecutor
    {
        private readonly ILogger deviceCodeLogger;
        private readonly Func<DeviceCodeResult, Task> deviceCodeResultCallback;

        public DeviceCodeCommandExecutor(
            IHttpClientFactory httpClientFactory,
            IOptions<AcquireTokenOptions> acquireTokenOptions,
            IOptions<PublicClientApplicationOptions> appOptions,
            MsalTokenCacheStorageProvider cacheStorageProvider,
            ILoggerFactory? loggerFactory = null)
            : base(httpClientFactory, acquireTokenOptions, appOptions,
                  cacheStorageProvider, loggerFactory)
        {
            loggerFactory ??= Microsoft.Extensions.Logging.Abstractions
                .NullLoggerFactory.Instance;

            string deviceCodeCategory = typeof(DeviceCodeResult).Namespace +
                "." + nameof(DeviceCodeResult.DeviceCode);
            deviceCodeLogger = loggerFactory.CreateLogger(deviceCodeCategory);

            deviceCodeResultCallback = DeviceCodeUserInteractionCallback;
        }

        protected override Task<AuthenticationResult> ExecuteAcquireToken(
            IPublicClientApplication app, AcquireTokenOptions options,
            CancellationToken cancelToken = default)
        {
            _ = app ?? throw new ArgumentNullException(nameof(app));

            var scopes = options?.Scopes ?? Enumerable.Empty<string>();
            var auth = app.AcquireTokenWithDeviceCode(scopes, deviceCodeResultCallback);
            return auth.ExecuteAsync(cancelToken);
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
