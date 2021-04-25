using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace THNETII.Msal.Extensions.UserSecrets
{
    public class MsalTokenCacheUserSecretsProvider : MsalTokenCacheProvider
    {
        private readonly string userSecretsDirectory;
        private readonly string userSecretsUserTokenFilePath;
        private readonly string userSecretsAppTokenFilePath;

        private readonly ILogger<MsalTokenCacheProvider> logger;

        public MsalTokenCacheUserSecretsProvider(string userSecretsId,
            IDataProtectionProvider? dpProvider = null,
            ILogger<MsalTokenCacheProvider>? logger = null)
            : this(new UserSecretsIdAttribute(userSecretsId), dpProvider)
        {
            this.logger = logger ?? Microsoft.Extensions.Logging
                .Abstractions.NullLogger<MsalTokenCacheProvider>.Instance;
        }

        public MsalTokenCacheUserSecretsProvider(
            UserSecretsIdAttribute? userSecretsIdAttribute = null,
            IDataProtectionProvider? dpProvider = null,
            ILogger<MsalTokenCacheProvider>? logger = null)
            : base(dpProvider)
        {
            this.logger = logger ?? Microsoft.Extensions.Logging
                .Abstractions.NullLogger<MsalTokenCacheProvider>.Instance;

            if (userSecretsIdAttribute?.UserSecretsId is not { Length: > 0 })
            {
                userSecretsIdAttribute = Assembly.GetEntryAssembly()?
                    .GetCustomAttribute<UserSecretsIdAttribute>();
            }
            if (userSecretsIdAttribute?.UserSecretsId is not { Length: > 0 })
            {
                userSecretsIdAttribute = GetType().Assembly
                    .GetCustomAttribute<UserSecretsIdAttribute>();
            }
            if (userSecretsIdAttribute?.UserSecretsId is not { Length: > 0 })
            {
                userSecretsIdAttribute = typeof(MsalTokenCacheUserSecretsProvider).Assembly
                    .GetCustomAttribute<UserSecretsIdAttribute>();
            }

            _ = userSecretsIdAttribute ?? throw new ArgumentNullException(nameof(userSecretsIdAttribute));
            if (userSecretsIdAttribute.UserSecretsId is not { Length: > 0 } userSecretsId)
                throw new ArgumentException(
                    message: default,
                    paramName: nameof(userSecretsIdAttribute)
                    );

            string userSecretsFilePath = PathHelper.GetSecretsPathFromSecretsId(
                userSecretsId);
            userSecretsDirectory = Path.GetDirectoryName(userSecretsFilePath)!;
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(userSecretsDirectory) == false);
            userSecretsUserTokenFilePath = Path.Combine(userSecretsDirectory, "msal.user-token-cache.dat");
            userSecretsAppTokenFilePath = Path.Combine(userSecretsDirectory, "msal.app-token-cache.dat");
        }

        protected override async Task<byte[]> GetMsalCacheData(
            TokenCacheNotificationArgs context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            var filePath = context.IsApplicationCache
                ? userSecretsAppTokenFilePath
                : userSecretsUserTokenFilePath;

            return await PerformRetryableFileOperation(filePath, default, readFileOperation!)
                .ConfigureAwait(continueOnCapturedContext: false)
                ?? Array.Empty<byte>();
        }

        protected override async Task StoreMsalCacheData(
            TokenCacheNotificationArgs context,
            byte[] msalTokenCacheData)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            if (msalTokenCacheData is not { Length: > 0 })
                return;

            var filePath = context.IsApplicationCache
                ? userSecretsAppTokenFilePath
                : userSecretsUserTokenFilePath;

            await PerformRetryableFileOperation(filePath, msalTokenCacheData, writeFileOperation!)
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        private async Task<byte[]?> PerformRetryableFileOperation(string filePath,
            byte[]? buffer, Func<string, byte[]?, Task<byte[]?>> ioOperation)
        {
            for (int i = 0; i < 10; i++,
                await Task.Delay(TimeSpan.FromMilliseconds(25))
                .ConfigureAwait(continueOnCapturedContext: false))
            {
                try
                {
                    return await ioOperation(filePath, buffer)
                        .ConfigureAwait(continueOnCapturedContext: false);
                }
                catch (FileNotFoundException fileNotFoundExcept)
                {
                    logger.LogWarning(
                        new EventId(fileNotFoundExcept.HResult, nameof(FileNotFoundException)),
                        fileNotFoundExcept.Message);
                    return buffer;
                }
                catch (DirectoryNotFoundException dirNotFoundExcept)
                {
                    logger.LogWarning(
                        new EventId(dirNotFoundExcept.HResult, nameof(DirectoryNotFoundException)),
                        dirNotFoundExcept.Message);
                    return buffer;
                }
                catch (IOException ioExcept)
                {
                    if (i < (10 - 1))
                    {
                        logger.LogDebug(
                            new EventId(ioExcept.HResult, ioExcept.GetType().Name),
                            ioExcept.Message
                            );
                        continue;
                    }

                    logger.LogError(ioExcept, "Unable to perform file operation for MSAL Token cache file");
                    throw;
                }
            }

            throw new InvalidOperationException();
        }

        private static Task<byte[]> ReadFileOperationAsync(string filePath,
            byte[]? msalCacheData)
        {
#if NET461 || NETSTANDARD2_0
            msalCacheData = File.ReadAllBytes(filePath);
            return Task.FromResult(msalCacheData);
#else
            return File.ReadAllBytesAsync(filePath);
#endif
        }

        private readonly Func<string, byte[]?, Task<byte[]>> readFileOperation =
            ReadFileOperationAsync;

        private static async Task<byte[]?> WriteFileOperationAsync(string filePath,
            byte[] msalCacheData)
        {
#if NET461 || NETSTANDARD2_0
            File.WriteAllBytes(filePath, msalCacheData);
            return await Task.FromResult<byte[]?>(null)
                .ConfigureAwait(continueOnCapturedContext: false);
#else
            await File.WriteAllBytesAsync(filePath, msalCacheData)
                .ConfigureAwait(continueOnCapturedContext: false);
            return null;
#endif
        }

        private readonly Func<string, byte[], Task<byte[]?>> writeFileOperation =
            WriteFileOperationAsync;
    }
}
