using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Microsoft.AspNetCore.DataProtection;

namespace Microsoft.Identity.Client
{
    public abstract class MsalTokenCacheProvider
    {
        private readonly IDataProtector? dataProtector;

        private readonly Func<TokenCacheNotificationArgs, Task> onBeforeAccess;
        private readonly Func<TokenCacheNotificationArgs, Task> onAfterAccess;

        public MsalTokenCacheProvider(IDataProtectionProvider? dpProvider)
        {
            if (dpProvider != null)
            {
                string protectPurpose = typeof(ITokenCacheSerializer).FullName +
                        "." + nameof(ITokenCacheSerializer.SerializeMsalV3);
                dataProtector = dpProvider.CreateProtector(protectPurpose);
            }

            onBeforeAccess = OnBeforeAccess;
            onAfterAccess = OnAfterAccess;
        }

        protected abstract Task<byte[]> GetMsalCacheData(TokenCacheNotificationArgs context);

        protected abstract Task StoreMsalCacheData(
            TokenCacheNotificationArgs context,
            byte[] msalTokenCacheData);

        private async Task OnBeforeAccess(TokenCacheNotificationArgs context)
        {
            var cacheProtected = await GetMsalCacheData(context)
                .ConfigureAwait(continueOnCapturedContext: false);
            if (cacheProtected is not { Length: > 0 })
                return;

            // Pinning array for best-effort clearing of unprotected secrets
            // in memory
            var cacheHandle = GCHandle.Alloc(
                dataProtector is null
                ? cacheProtected
                : dataProtector.Unprotect(cacheProtected),
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

        private async Task OnAfterAccess(TokenCacheNotificationArgs context)
        {
            if (context.HasStateChanged)
            {
                // Pinning array for best-effort clearing of unprotected secrets
                // in memory
                var cacheHandle = GCHandle.Alloc(
                    context.TokenCache.SerializeMsalV3(),
                    GCHandleType.Pinned);
                if (dataProtector is not null)
                {
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

                    await StoreMsalCacheData(context, cacheProtected)
                        .ConfigureAwait(continueOnCapturedContext: false);
                }
                else
                {
                    var cacheRaw = (byte[])cacheHandle.Target!;
                    try
                    {
                        await StoreMsalCacheData(context, cacheRaw)
                            .ConfigureAwait(continueOnCapturedContext: false);
                    }
                    finally
                    {
                        Array.Clear(cacheRaw, 0, cacheRaw.Length);
                        cacheHandle.Free();
                    }
                }
            }
            else
                return;
        }

        public virtual void RegisterCache(ITokenCache cache)
        {
            _ = cache ?? throw new ArgumentNullException(nameof(cache));
            cache.SetBeforeAccessAsync(onBeforeAccess);
            cache.SetAfterAccessAsync(onAfterAccess);
        }
    }
}
