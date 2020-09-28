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
    public class SilentCommandExecutor
        : PublicClientApplicationAcquireTokenExecutor
    {
        private readonly SilentAcquireTokenOptions silentOptions;

        public SilentCommandExecutor(IHttpClientFactory httpClientFactory,
            IOptions<SilentAcquireTokenOptions> acquireTokenOptions,
            IOptions<PublicClientApplicationOptions> appOptions,
            MsalTokenCacheStorageProvider cacheStorageProvider,
            ILoggerFactory? loggerFactory = null)
            : base(httpClientFactory, acquireTokenOptions, appOptions,
                  cacheStorageProvider, loggerFactory)
        {
            silentOptions = acquireTokenOptions.Value;
        }

        protected override async Task<AuthenticationResult> ExecuteAcquireToken(
            IPublicClientApplication app, AcquireTokenOptions options,
            CancellationToken cancelToken = default)
        {
            _ = app ?? throw new ArgumentNullException(nameof(app));

            var account = await app
                .GetAccountAsync(silentOptions.AccountIdentifier)
                .ConfigureAwait(continueOnCapturedContext: false);

            var scopes = options?.Scopes ?? Enumerable.Empty<string>();
            var auth = app.AcquireTokenSilent(scopes, account);
            if (silentOptions.ForceRefresh.HasValue)
            {
                bool forceRefresh = silentOptions.ForceRefresh.Value;
                auth.WithForceRefresh(forceRefresh);
            }
            return await auth.ExecuteAsync(cancelToken)
                .ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}
