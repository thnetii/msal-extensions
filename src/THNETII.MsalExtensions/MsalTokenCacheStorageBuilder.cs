using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client.Extensions.Msal;

using THNETII.Logging.DiagnosticsForwarding;

using static Microsoft.Identity.Client.MsalTokenCacheStorageOptions;

namespace Microsoft.Identity.Client
{
    public class MsalTokenCacheStorageBuilder
    {
        private static readonly string assemblyVersion =
            typeof(MsalTokenCacheStorageBuilder).Assembly
            .GetName().Version!.ToString();

        private string? clientId;
        private string cacheFileName = DefaultCacheFileName;
        private string cacheDirectory = default!;
        private Tuple<int, int>? customLockRetry;

        private bool disableLinuxKeyring;
        private bool useLinuxInMemoryKeyring;
        private string linuxKeyringSchemaName = default!;

        private bool disableMacKeychain;
        private string macKeyChainServiceName = default!;
        private string macKeyChainAccountName = default!;

        private bool disableLinuxUnprotectedFileFallback;

        private TraceSource? traceSourceLogger;

        public static MsalTokenCacheStorageBuilder Create() =>
            new MsalTokenCacheStorageBuilder();

        public static MsalTokenCacheStorageBuilder Create(IServiceProvider serviceProvider)
        {
            var builder = new MsalTokenCacheStorageBuilder();
            if (serviceProvider is null)
                return builder;

            MsalTokenCacheStorageOptions options = serviceProvider
                .GetService<IOptions<MsalTokenCacheStorageOptions>>()?.Value
                ?? serviceProvider.GetService<MsalTokenCacheStorageOptions>();
            if (!(options is null))
                builder.WithOptions(options);
            ILoggerFactory? loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            if (!(loggerFactory is null))
                builder.WithLoggerFactory(loggerFactory);

            return builder;
        }

        public static MsalTokenCacheStorageBuilder CreateWithOptions(MsalTokenCacheStorageOptions options)
        {
            var builder = new MsalTokenCacheStorageBuilder();
            builder.WithOptions(options);
            return builder;
        }

        private MsalTokenCacheStorageBuilder() : base()
        {
            WithApplicationAssembly(GetType().Assembly);
        }

        public MsalTokenCacheStorageBuilder WithOptions(MsalTokenCacheStorageOptions options)
        {
            _ = options ?? throw new ArgumentNullException(nameof(options));



            return this;
        }

        public MsalTokenCacheStorageBuilder WithClientId(string clientId)
        {
            this.clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            return this;
        }

        public MsalTokenCacheStorageBuilder WithApplicationAssembly(Assembly assembly)
        {
            _ = assembly ?? throw new ArgumentNullException(nameof(assembly));

            cacheDirectory = GetUserSecretsCacheDirectory(assembly);
            AssemblyName assemblyName = assembly.GetName();
            linuxKeyringSchemaName = macKeyChainServiceName = assemblyName.Name ?? string.Empty;
            macKeyChainAccountName = assembly.FullName ?? string.Empty;

            return this;
        }

        public MsalTokenCacheStorageBuilder WithLoggerFactory(ILoggerFactory loggerFactory)
        {
            loggerFactory ??= Microsoft.Extensions.Logging.Abstractions
                .NullLoggerFactory.Instance;

            const string loggerName = "Microsoft.Identity.Client.Extensions.TraceSource";
            traceSourceLogger = new TraceSource(loggerName)
            {
                Switch = { Level = SourceLevels.All },
                Listeners =
                {
                    new TraceSourceLogForwarder(loggerFactory, loggerName)
                }
            };

            return this;
        }

        public async Task<MsalCacheHelper> BuildAsync()
        {
            StorageCreationPropertiesBuilder storagePropsBuilder = CreateStorageCreationPropertiesBuilder();

            var storageProps = storagePropsBuilder.Build();
            var cacheHelper = await MsalCacheHelper
                .CreateAsync(storageProps, traceSourceLogger)
                .ConfigureAwait(continueOnCapturedContext: false);
            try
            {
                cacheHelper.VerifyPersistence();
            }
            catch (MsalCachePersistenceException except)
            when (!disableLinuxUnprotectedFileFallback)
            {
                traceSourceLogger?.TraceEvent(TraceEventType.Warning, except.HResult,
                    "Unable to verify Cache Persistence, using unprotected file fallback. {0}",
                    except.Message);
                storagePropsBuilder = CreateStorageCreationPropertiesBuilder()
                    .WithLinuxUnprotectedFile();
                storageProps = storagePropsBuilder.Build();
                cacheHelper = await MsalCacheHelper
                    .CreateAsync(storageProps, traceSourceLogger)
                    .ConfigureAwait(continueOnCapturedContext: false);
                cacheHelper.VerifyPersistence();
            }
            return cacheHelper;
        }

        private StorageCreationPropertiesBuilder CreateStorageCreationPropertiesBuilder()
        {
            var storagePropsBuilder = new StorageCreationPropertiesBuilder(
                            cacheFileName, cacheDirectory, clientId);
            if (!(customLockRetry is null))
            {
                var (lockRetryDelay, lockRetryCount) = customLockRetry;
                storagePropsBuilder.CustomizeLockRetry(
                    lockRetryDelay,
                    lockRetryCount
                    );
            }
            if (!disableLinuxKeyring)
            {
                string collection = useLinuxInMemoryKeyring
                    ? MsalCacheHelper.LinuxKeyRingSessionCollection
                    : MsalCacheHelper.LinuxKeyRingDefaultCollection;
                string secretLabel = $"MSAL token cache (ClientID: {clientId})";
                KeyValuePair<string, string> versionAttr =
                    new KeyValuePair<string, string>("Version", assemblyVersion);
                KeyValuePair<string, string> msalAttr =
                    new KeyValuePair<string, string>("ProductGroup",
                        typeof(IClientApplicationBase).Assembly.FullName!);
                storagePropsBuilder.WithLinuxKeyring(
                    linuxKeyringSchemaName, collection, secretLabel,
                    versionAttr, msalAttr);
            }
            if (!disableMacKeychain)
            {
                storagePropsBuilder.WithMacKeyChain(
                    macKeyChainServiceName,
                    macKeyChainAccountName);
            }

            return storagePropsBuilder;
        }

        private static string GetUserSecretsCacheDirectory(Assembly cacheAssembly)
        {
            var userIdAttr = cacheAssembly.GetCustomAttribute<UserSecretsIdAttribute>();
            string? userSecretsId = userIdAttr?.UserSecretsId;
            // Propagate thrown exception if userSecretsId is null or empty
            string userSecretsFilePath = PathHelper.GetSecretsPathFromSecretsId(userSecretsId!);
            return Path.GetDirectoryName(userSecretsFilePath)!;
        }
    }
}
