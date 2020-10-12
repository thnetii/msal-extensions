using System;
using System.Threading.Tasks;

namespace Microsoft.Identity.Client
{
    public static class MsalTokenCacheStorageExtensions
    {
        public static Task<IPublicClientApplication> BuildWithTokenCacheStorageAsync(
            this PublicClientApplicationBuilder builder,
            IServiceProvider serviceProvider,
            Action<MsalTokenCacheStorageBuilder>? configureStorage = null)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
            var application = builder.Build();
            return ConfigureTokenCacheStorageAsync(application, serviceProvider,
                configureStorage);
        }

        public static Task<IConfidentialClientApplication> BuildWithTokenCacheStorageAsync(
            this ConfidentialClientApplicationBuilder builder,
            IServiceProvider serviceProvider,
            Action<MsalTokenCacheStorageBuilder>? configureStorage = null)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
            var application = builder.Build();
            return ConfigureTokenCacheStorageAsync(application, serviceProvider,
                configureStorage);
        }

        private static async Task<TApplication> ConfigureTokenCacheStorageAsync<TApplication>(
            TApplication application, IServiceProvider serviceProvider,
            Action<MsalTokenCacheStorageBuilder>? configureStorage = null)
            where TApplication : IClientApplicationBase
        {
            var storageBuilder = MsalTokenCacheStorageBuilder.Create(
                serviceProvider);
            storageBuilder.WithClientId(application.AppConfig.ClientId);
            configureStorage?.Invoke(storageBuilder);
            var storageCache = await storageBuilder.BuildAsync()
                .ConfigureAwait(continueOnCapturedContext: false);
            storageCache.RegisterCache(application.UserTokenCache);
            return application;
        }
    }
}
