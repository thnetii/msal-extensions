using System;
using System.Linq;
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
        private readonly AcquireTokenOptions acquireTokenOptions;

        public DeviceCodeCommandExecutor(
            ClientApplicationFactory clientApplicationFactory,
            IOptions<AcquireTokenOptions> acquireTokenOptions,
            ILoggerFactory? loggerFactory = null)
            : base(clientApplicationFactory, loggerFactory)
        {
            this.acquireTokenOptions = acquireTokenOptions?.Value
                ?? throw new ArgumentNullException(nameof(acquireTokenOptions));
            loggerFactory ??= Microsoft.Extensions.Logging.Abstractions
                .NullLoggerFactory.Instance;

            string deviceCodeCategory = typeof(DeviceCodeResult).Namespace +
                "." + nameof(DeviceCodeResult.DeviceCode);
            deviceCodeLogger = loggerFactory.CreateLogger(deviceCodeCategory);

            deviceCodeResultCallback = DeviceCodeUserInteractionCallback;
        }

        protected override async Task<AuthenticationResult> ExecuteAcquireToken(
            CancellationToken cancelToken = default)
        {
            var scopes = acquireTokenOptions?.Scopes ?? Enumerable.Empty<string>();
            var application = await CreatePublicClientApplication()
                .ConfigureAwait(continueOnCapturedContext: false);
            var auth = application
                .AcquireTokenWithDeviceCode(scopes, deviceCodeResultCallback);
            return await auth.ExecuteAsync(cancelToken)
                .ConfigureAwait(continueOnCapturedContext: false);
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
