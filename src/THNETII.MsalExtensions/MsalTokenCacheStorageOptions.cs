using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Identity.Client
{
    public class MsalTokenCacheStorageOptions
    {
        internal const string DefaultCacheFileName = "msal_cache.dat";

        public string? ClientId { get; set; } = null!;
        public string? CacheFileName { get; set; } = DefaultCacheFileName;
        public string? CacheDirectory { get; set; }
        public Assembly? ApplicationAssembly { get; set; } = Assembly.GetEntryAssembly();

        public bool DisableLinuxKeyring { get; set; }
        public bool DisableMacKeyChain { get; set; }
        public bool UseInMemoryKeyring { get; set; }
    }
}
