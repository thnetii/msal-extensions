using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client.Extensions.Msal;

using THNETII.Logging.DiagnosticsForwarding;

using static Microsoft.Identity.Client.MsalTokenCacheStorageOptions;

namespace Microsoft.Identity.Client
{
    public class MsalTokenCacheStorageOptions
    {
        internal const string DefaultCacheFileName = "msal_cache.dat";

        public string ClientId { get; set; } = null!;
        public string CacheFileName { get; set; } = DefaultCacheFileName;
        public string? CacheDirectory { get; set; }
        public Assembly? ApplicationAssembly { get; set; } = Assembly.GetEntryAssembly();

        public bool DisableLinuxKeyring { get; set; }
        public bool DisableMacKeyChain { get; set; }
        public bool UseInMemoryKeyring { get; set; }
    }

    public sealed class MsalTokenCacheStorageProvider : IDisposable
    {
        private static readonly string assemblyVersion =
            typeof(MsalTokenCacheStorageProvider).Assembly
            .GetName().Version!.ToString();

        private bool useLinuxUnprotectedFile;
        private readonly IDisposable optionsChangeRegistration;
        private readonly TraceSource traceSourceLogger;
        private readonly IOptionsMonitor<MsalTokenCacheStorageOptions> optionsMonitor;
        private StorageCreationProperties storageProperties;

        public MsalTokenCacheStorageProvider(
            IOptionsMonitor<MsalTokenCacheStorageOptions> optionsMonitor,
            ILoggerFactory? loggerFactory = null)
        {
            this.optionsMonitor = optionsMonitor
                ?? throw new ArgumentNullException(nameof(optionsMonitor));
            loggerFactory ??= Microsoft.Extensions.Logging.Abstractions
                .NullLoggerFactory.Instance;

            storageProperties = null!;
            optionsChangeRegistration = optionsMonitor
                .OnChange(UpdateStorageCreationProperties);
            UpdateStorageCreationProperties(optionsMonitor.CurrentValue);

            const string loggerName = "Microsoft.Identity.Client.Extensions.TraceSource";
            traceSourceLogger = new TraceSource(loggerName)
            {
                Switch = { Level = SourceLevels.All },
                Listeners =
                {
                    new TraceSourceLogForwarder(loggerFactory, loggerName)
                }
            };
        }

        private void UpdateStorageCreationProperties(
            MsalTokenCacheStorageOptions options)
        {
            string clientId = options.ClientId ?? Guid.NewGuid().ToString();
            string cacheFileName = options.CacheFileName ?? DefaultCacheFileName;
            Assembly cacheAssembly = options.ApplicationAssembly
                ?? Assembly.GetEntryAssembly()
                ?? GetType().Assembly;
            AssemblyName cacheAssemblyName = cacheAssembly.GetName();
            string cacheDirectory = options.CacheDirectory ??
                GetUserSecretsCacheDirectory(cacheAssembly);

            var storagePropsBuilder = new StorageCreationPropertiesBuilder(
                cacheFileName, cacheDirectory, clientId);
            if (!options.DisableLinuxKeyring)
            {
                string schemaName = cacheAssemblyName.Name ?? string.Empty;
                string collection = options.UseInMemoryKeyring
                    ? MsalCacheHelper.LinuxKeyRingSessionCollection
                    : MsalCacheHelper.LinuxKeyRingDefaultCollection;
                string secretLabel = $"MSAL token cache (ClientID: {clientId})";
                KeyValuePair<string, string> versionAttr =
                    new KeyValuePair<string, string>("Version", assemblyVersion);
                KeyValuePair<string, string> msalAttr =
                    new KeyValuePair<string, string>("ProductGroup",
                        typeof(IClientApplicationBase).Assembly.FullName!);

                storagePropsBuilder.WithLinuxKeyring(
                    schemaName, collection, secretLabel,
                    versionAttr, msalAttr);
            }
            if (!options.DisableMacKeyChain)
            {
                string serviceName = cacheAssemblyName.Name ?? string.Empty;
                string accountName = cacheAssembly.FullName ?? string.Empty;

                storagePropsBuilder.WithMacKeyChain(
                    serviceName, accountName);
            }
            if (useLinuxUnprotectedFile)
            {
                storagePropsBuilder.WithLinuxUnprotectedFile();
            }

            storageProperties = storagePropsBuilder.Build();
        }

        private static string GetUserSecretsCacheDirectory(Assembly cacheAssembly)
        {
            var userIdAttr = cacheAssembly.GetCustomAttribute<UserSecretsIdAttribute>();
            string userSecretsId = userIdAttr?.UserSecretsId ??
                "UnknownUserSecretsId";
            string userSecretsFilePath = PathHelper.GetSecretsPathFromSecretsId(userSecretsId);
            return Path.GetDirectoryName(userSecretsFilePath)!;
        }

        public Task<MsalCacheHelper> GetCachePersistanceHelper() =>
            GetCachePersistanceHelperInternal();

        private async Task<MsalCacheHelper> GetCachePersistanceHelperInternal(
            bool isRetry = false)
        {
            var storageProps = storageProperties;
            var helper = await MsalCacheHelper
                .CreateAsync(storageProps, traceSourceLogger)
                .ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                helper.VerifyPersistence();
                return helper;
            }
            catch (MsalCachePersistenceException) when (!isRetry)
            {
                useLinuxUnprotectedFile = true;
                UpdateStorageCreationProperties(optionsMonitor.CurrentValue);
                return await GetCachePersistanceHelperInternal(isRetry: true)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public void Dispose()
        {
            optionsChangeRegistration.Dispose();
        }
    }
}
