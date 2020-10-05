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
            ClientApplicationFactory clientApplicationFactory,
            IOptions<IntegratedWindowsAuthenticationAcquireTokenOptions> acquireTokenOptions,
            ILoggerFactory? loggerFactory = null)
            : base(clientApplicationFactory, loggerFactory)
        {
            this.acquireTokenOptions = acquireTokenOptions?.Value
                ?? throw new ArgumentNullException(nameof(acquireTokenOptions));
        }

        protected override async Task<AuthenticationResult> ExecuteAcquireToken(
            CancellationToken cancelToken = default)
        {
            var scopes = acquireTokenOptions?.Scopes ?? Enumerable.Empty<string>();
            var application = await CreatePublicClientApplication()
                .ConfigureAwait(continueOnCapturedContext: false);
            var auth = application
                .AcquireTokenByIntegratedWindowsAuth(scopes);
            if (acquireTokenOptions?.Username is string username &&
                !string.IsNullOrEmpty(username))
                auth.WithUsername(username);
            return await auth.ExecuteAsync(cancelToken)
                .ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}
