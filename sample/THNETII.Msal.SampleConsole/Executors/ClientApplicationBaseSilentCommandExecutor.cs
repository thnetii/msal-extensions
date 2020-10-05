using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public class ClientApplicationBaseSilentCommandExecutor
        : ClientApplicationBaseAcquireTokenExecutor
    {
        private readonly SilentAcquireTokenOptions silentOptions;

        protected ClientApplicationBaseSilentCommandExecutor(
            ClientApplicationFactory clientApplicationFactory,
            IOptions<SilentAcquireTokenOptions> acquireTokenOptions,
            ILoggerFactory? loggerFactory = null)
            : base(clientApplicationFactory, loggerFactory)
        {
            silentOptions = acquireTokenOptions?.Value
                ?? throw new ArgumentNullException(nameof(acquireTokenOptions));
        }

        protected override sealed async Task<AuthenticationResult> ExecuteAcquireToken(
            CancellationToken cancelToken = default)
        {
            var application = await CreateClientApplication()
                .ConfigureAwait(continueOnCapturedContext: false);
            var account = await application
                .GetAccountAsync(silentOptions.AccountIdentifier)
                .ConfigureAwait(continueOnCapturedContext: false);

            var scopes = silentOptions.Scopes ?? Enumerable.Empty<string>();
            var auth = application.AcquireTokenSilent(scopes, account);
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
