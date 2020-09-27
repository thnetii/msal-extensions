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
    public class IntegratedWindowsAuthenticationCommandExecutor
        : ICommandLineExecutor
    {
        private readonly IPublicClientApplication app;
        private readonly AcquireTokenOptions acquireTokenOptions;

        public IntegratedWindowsAuthenticationCommandExecutor(
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

            var appBuilder = PublicClientApplicationBuilder
                .CreateWithApplicationOptions(opts)
                .WithLoggerFactory(loggerFactory);
            if (!(httpClientFactory is null))
                appBuilder.WithHttpClientFactory(httpClientFactory);
            app = appBuilder.Build();
        }

        public async Task<int> RunAsync(CancellationToken cancelToken = default)
        {
            var scopes = acquireTokenOptions.Scopes ?? Enumerable.Empty<string>();
            var username = acquireTokenOptions.Username;

            var flowBuilder = app
                .AcquireTokenByIntegratedWindowsAuth(scopes);
            if (!string.IsNullOrEmpty(username))
                flowBuilder = flowBuilder.WithUsername(username);

            var authResult = await flowBuilder.ExecuteAsync(cancelToken)
                .ConfigureAwait(continueOnCapturedContext: false);

            _ = authResult;

            return 0;
        }
    }
}
