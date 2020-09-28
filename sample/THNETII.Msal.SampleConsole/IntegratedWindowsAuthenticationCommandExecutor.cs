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
    public class IntegratedWindowsAuthenticationCommandExecutor
        : PublicClientApplicationAcquireTokenExecutor
    {
        public IntegratedWindowsAuthenticationCommandExecutor(
            IHttpClientFactory httpClientFactory,
            IOptions<IntegratedWindowsAuthenticationAcquireTokenOptions> acquireTokenOptions,
            IOptions<PublicClientApplicationOptions> appOptions,
            MsalTokenCacheStorageProvider cacheStorageProvider,
            ILoggerFactory? loggerFactory = null)
            : base(httpClientFactory, acquireTokenOptions, appOptions, cacheStorageProvider, loggerFactory) { }

        protected override Task<AuthenticationResult> ExecuteAcquireToken(
            IPublicClientApplication app,
            AcquireTokenOptions options,
            CancellationToken cancelToken = default)
        {
            _ = app ?? throw new ArgumentNullException(nameof(app));

            var iwaOptions = options as IntegratedWindowsAuthenticationAcquireTokenOptions;

            var scopes = iwaOptions?.Scopes ?? Enumerable.Empty<string>();
            var auth = app.AcquireTokenByIntegratedWindowsAuth(scopes);
            if (iwaOptions?.Username is string username &&
                !string.IsNullOrEmpty(username))
                auth.WithUsername(username);
            return auth.ExecuteAsync(cancelToken);
        }
    }
}
