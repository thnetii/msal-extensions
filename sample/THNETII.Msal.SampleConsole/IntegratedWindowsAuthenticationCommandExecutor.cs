using System;
using System.Linq;
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
        private readonly IntegratedWindowsAuthenticationAcquireTokenOptions acquireTokenOptions;

        public IntegratedWindowsAuthenticationCommandExecutor(
            IServiceProvider serviceProvider,
            IOptions<IntegratedWindowsAuthenticationAcquireTokenOptions> acquireTokenOptions,
            MsalTokenCacheStorageProvider cacheStorageProvider,
            ILoggerFactory? loggerFactory = null)
            : base(serviceProvider, cacheStorageProvider, loggerFactory)
        {
            this.acquireTokenOptions = acquireTokenOptions?.Value
                ?? throw new ArgumentNullException(nameof(acquireTokenOptions));
        }

        protected override Task<AuthenticationResult> ExecuteAcquireToken(
            CancellationToken cancelToken = default)
        {
            var scopes = acquireTokenOptions?.Scopes ?? Enumerable.Empty<string>();
            var auth = Application
                .AcquireTokenByIntegratedWindowsAuth(scopes);
            if (acquireTokenOptions?.Username is string username &&
                !string.IsNullOrEmpty(username))
                auth.WithUsername(username);
            return auth.ExecuteAsync(cancelToken);
        }
    }
}
