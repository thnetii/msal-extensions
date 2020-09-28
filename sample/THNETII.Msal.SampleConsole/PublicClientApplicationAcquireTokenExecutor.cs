using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole
{
    public abstract class PublicClientApplicationAcquireTokenExecutor :
        ICommandLineExecutor
    {
        private readonly IPublicClientApplication app;
        private readonly ILogger logger;
        private readonly AcquireTokenOptions acquireTokenOptions;
        private readonly MsalTokenCacheStorageProvider cacheStorageProvider;

        public PublicClientApplicationAcquireTokenExecutor(
            IHttpClientFactory httpClientFactory,
            IOptions<AcquireTokenOptions> acquireTokenOptions,
            IOptions<PublicClientApplicationOptions> appOptions,
            MsalTokenCacheStorageProvider cacheStorageProvider,
            ILoggerFactory? loggerFactory = null)
        {
            var opts = appOptions?.Value
                ?? throw new ArgumentNullException(nameof(appOptions));

            this.acquireTokenOptions = acquireTokenOptions?.Value ??
                throw new ArgumentNullException(nameof(acquireTokenOptions));

            this.cacheStorageProvider = cacheStorageProvider
                ?? throw new ArgumentNullException(nameof(cacheStorageProvider));

            loggerFactory ??= Microsoft.Extensions.Logging.Abstractions
                .NullLoggerFactory.Instance;
            logger = loggerFactory.CreateLogger(GetType());

            var appBuilder = PublicClientApplicationBuilder
                .CreateWithApplicationOptions(opts)
                .WithLoggerFactory(loggerFactory);
            if (!(httpClientFactory is null))
                appBuilder.WithHttpClientFactory(httpClientFactory);
            app = appBuilder.Build();
        }

        public async Task<int> RunAsync(CancellationToken cancelToken = default)
        {
            var cacheHelper = await cacheStorageProvider.GetCachePersistanceHelper()
                .ConfigureAwait(continueOnCapturedContext: false);
            cacheHelper.RegisterCache(app.UserTokenCache);

            AuthenticationResult authResult;
            try
            {
                authResult = await ExecuteAcquireToken(app, acquireTokenOptions,
                    cancelToken)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (OperationCanceledException cancelExcept)
            {
                logger.LogError(cancelExcept.Message);
                return cancelExcept.HResult switch
                {
                    0 => 1,
                    int code => code
                };
            }

            _ = authResult;

            return 0;
        }

        protected abstract Task<AuthenticationResult> ExecuteAcquireToken(
            IPublicClientApplication app, AcquireTokenOptions options,
            CancellationToken cancelToken = default);
    }
}
