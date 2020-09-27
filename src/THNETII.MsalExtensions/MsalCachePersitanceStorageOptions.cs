using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Microsoft.Identity.Client
{
    public class MsalCachePersitanceStorageOptions
    {
    }

    public sealed class MsalCachePersistanceStorageProvider : IDisposable
    {
        private readonly IDisposable optionsChangeRegistration;
        private readonly TraceSource traceSourceLogger;
        private StorageCreationProperties storageProperties;

        public MsalCachePersistanceStorageProvider(
            IOptionsMonitor<MsalCachePersitanceStorageOptions> optionsMonitor)
        {
            _ = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));

            storageProperties = null!;
            optionsChangeRegistration = optionsMonitor
                .OnChange(UpdateStorageCreationProperties);
            UpdateStorageCreationProperties(optionsMonitor.CurrentValue);

            traceSourceLogger = new TraceSource(null);
            traceSourceLogger.Listeners.Add(new TraceSourceLogForwarder());
        }

        private void UpdateStorageCreationProperties(
            MsalCachePersitanceStorageOptions options)
        {
            string cacheFileName = null;
            string cacheDirectory = null;
            string clientId = null;

            var storagePropsBuilder = new StorageCreationPropertiesBuilder(
                cacheFileName, cacheDirectory, clientId);
            if (true)
            {
                string schemaName = null;
                string collection = null;
                string secretLabel = null;
                KeyValuePair<string, string> attribute1 = default;
                KeyValuePair<string, string> attribute2 = default;

                storagePropsBuilder.WithLinuxKeyring(
                    schemaName, collection, secretLabel,
                    attribute1, attribute2);
            }
            if (true)
            {
                string serviceName = null;
                string accountName = null;

                storagePropsBuilder.WithMacKeyChain(
                    serviceName, accountName);
            }
            if (true)
            {
                storagePropsBuilder.WithLinuxUnprotectedFile();
            }

            storageProperties = storagePropsBuilder.Build();
        }

        public Task<MsalCacheHelper> GetCachePersistanceHelper()
        {
            var storageProps = storageProperties;
            return MsalCacheHelper.CreateAsync(storageProps, traceSourceLogger);
        }

        public void Dispose() => optionsChangeRegistration.Dispose();
    }

    public class TraceSourceLogForwarder : TraceListener
    {
        public override void Write(string message)
        {
            throw new NotImplementedException();
        }

        public override void WriteLine(string message)
        {
            throw new NotImplementedException();
        }
    }
}
