using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.Identity.Client
{
    public class MsalTokenCacheProvider
    {
        private readonly IDataProtector dataProtector;
        private readonly IDistributedCache distributedCache;

        private readonly Func<TokenCacheNotificationArgs, Task> onBeforeAccess;
        private readonly Func<TokenCacheNotificationArgs, Task> onAfterAccess;

        public MsalTokenCacheProvider(
            IDataProtectionProvider dpProvider,
            IDistributedCache distributedCache)
        {
            _ = dpProvider ?? throw new ArgumentNullException(nameof(dpProvider));
            this.distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));

            string protectPurpose = typeof(ITokenCacheSerializer).FullName +
                "." + nameof(ITokenCacheSerializer.SerializeMsalV3);
            dataProtector = dpProvider.CreateProtector(protectPurpose);

            onBeforeAccess = OnBeforeAccess;
            onAfterAccess = OnAfterAccess;
        }

        private async Task OnBeforeAccess(TokenCacheNotificationArgs context)
        {
            var cacheKey = GetCacheKey(context);
            var cacheProtected = await distributedCache.GetAsync(cacheKey)
                .ConfigureAwait(continueOnCapturedContext: false);
            if (cacheProtected is null)
                return;
            // Pinning array for best-effort clearing of unprotected secrets
            // in memory
            var cacheHandle = GCHandle.Alloc(
                dataProtector.Unprotect(cacheProtected),
                GCHandleType.Pinned);
            try
            {
                byte[] cacheRaw = (byte[])cacheHandle.Target!;
                context.TokenCache.DeserializeMsalV3(cacheRaw);
                Array.Clear(cacheRaw, 0, cacheRaw.Length);
            }
            finally
            {
                cacheHandle.Free();
            }
        }

        private Task OnAfterAccess(TokenCacheNotificationArgs context)
        {
            if (context.HasStateChanged)
            {
                // Pinning array for best-effort clearing of unprotected secrets
                // in memory
                var cacheHandle = GCHandle.Alloc(
                    context.TokenCache.SerializeMsalV3(),
                    GCHandleType.Pinned);
                byte[] cacheProtected;
                try
                {
                    var cacheRaw = (byte[])cacheHandle.Target!;
                    cacheProtected = dataProtector.Protect(cacheRaw);
                    Array.Clear(cacheRaw, 0, cacheRaw.Length);
                }
                finally
                {
                    cacheHandle.Free();
                }
                var cacheKey = GetCacheKey(context);
                return distributedCache.SetAsync(cacheKey, cacheProtected);
            }
            else
                return Task.CompletedTask;
        }

        private static string GetCacheKey(TokenCacheNotificationArgs context)
        {
            return (context.SuggestedCacheKey, context.Account, context.IsApplicationCache, context.ClientId) switch
            {
                (string key, _, _, _) => key,
                (_, { HomeAccountId: { Identifier: string id } }, _, _) => id,
                (_, _, true, string clientId) => clientId + "_" + nameof(IConfidentialClientApplication.AppTokenCache),
                _ => string.Empty,
            };
        }

        public void RegisterCache(ITokenCache cache)
        {
            _ = cache ?? throw new ArgumentNullException(nameof(cache));
            cache.SetBeforeAccessAsync(onBeforeAccess);
            cache.SetAfterAccessAsync(onAfterAccess);
        }
    }
}
