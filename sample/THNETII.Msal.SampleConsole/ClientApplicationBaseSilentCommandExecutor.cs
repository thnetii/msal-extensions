using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public abstract class ClientApplicationBaseSilentCommandExecutor
        : ClientApplicationBaseAcquireTokenExecutor
    {
        private readonly SilentAcquireTokenOptions silentOptions;

        protected ClientApplicationBaseSilentCommandExecutor(
            IOptions<SilentAcquireTokenOptions> acquireTokenOptions,
            MsalTokenCacheStorageProvider cacheStorageProvider,
            ILoggerFactory? loggerFactory = null)
            : base(cacheStorageProvider, loggerFactory)
        {
            silentOptions = acquireTokenOptions?.Value
                ?? throw new ArgumentNullException(nameof(acquireTokenOptions));
        }

        protected override async Task<AuthenticationResult> ExecuteAcquireToken(
            CancellationToken cancelToken = default)
        {
            var account = await BaseApplication
                .GetAccountAsync(silentOptions.AccountIdentifier)
                .ConfigureAwait(continueOnCapturedContext: false);

            var scopes = silentOptions.Scopes ?? Enumerable.Empty<string>();
            var auth = BaseApplication.AcquireTokenSilent(scopes, account);
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
