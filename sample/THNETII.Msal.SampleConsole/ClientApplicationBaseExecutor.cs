using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole
{
    public abstract class ClientApplicationBaseExecutor : ICommandLineExecutor
    {
        private readonly MsalTokenCacheStorageProvider cacheStorageProvider;

        protected ClientApplicationBaseExecutor(
            MsalTokenCacheStorageProvider cacheStorageProvider,
            ILoggerFactory? loggerFactory = null)
        {
            this.cacheStorageProvider = cacheStorageProvider
                ?? throw new ArgumentNullException(nameof(cacheStorageProvider));

            loggerFactory ??= Microsoft.Extensions.Logging.Abstractions
                .NullLoggerFactory.Instance;
            Logger = loggerFactory.CreateLogger(GetType());
        }

        protected ILogger Logger { get; }

        protected abstract IClientApplicationBase BaseApplication { get; }

        public async Task<int> RunAsync(CancellationToken cancelToken = default)
        {
            var cacheHelper = await cacheStorageProvider.GetCachePersistanceHelper()
                .ConfigureAwait(continueOnCapturedContext: false);
            cacheHelper.RegisterCache(BaseApplication.UserTokenCache);

            return await ExecuteAsync(cancelToken)
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        protected abstract Task<int> ExecuteAsync(CancellationToken cancelToken);
    }
}
