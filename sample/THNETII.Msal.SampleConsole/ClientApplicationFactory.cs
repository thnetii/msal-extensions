using System;
using System.CommandLine.Parsing;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public class ClientApplicationFactory
    {
        private readonly IServiceProvider serviceProvider;

        public ClientApplicationFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider
                ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        private void ConfigureBuilder<T>(AbstractApplicationBuilder<T> builder)
            where T : AbstractApplicationBuilder<T>
        {
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            if (!(httpClientFactory is null))
                builder.WithHttpClientFactory(httpClientFactory);
            if (!(loggerFactory is null))
                builder.WithLoggerFactory(loggerFactory);
        }

        public Task<IPublicClientApplication> CreatePublicClientApplication()
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<PublicClientApplicationOptions>>()
                .Value;
            var builder = PublicClientApplicationBuilder
                .CreateWithApplicationOptions(options);
            ConfigureBuilder(builder);
            return builder.BuildWithTokenCacheStorageAsync(
                serviceProvider, ConfigureUserTokenCacheStorage);
        }

        public Task<IConfidentialClientApplication> CreateConfidentialClientApplication()
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<ConfidentialClientApplicationOptions>>()
                .Value;
            var builder = ConfidentialClientApplicationBuilder
                .CreateWithApplicationOptions(options);
            ConfigureBuilder(builder);
            return builder.BuildWithTokenCacheStorageAsync(
                serviceProvider, ConfigureUserTokenCacheStorage);
        }

        private void ConfigureUserTokenCacheStorage(MsalTokenCacheStorageBuilder builder)
        {
            builder.WithApplicationAssembly(typeof(Program).Assembly);
        }

        public async Task<IClientApplicationBase> CreateClientApplication()
        {
            var argsParseResult = serviceProvider.GetRequiredService<ParseResult>();
            for (
                var cmdResult = argsParseResult.CommandResult;
                !(cmdResult is null);
                cmdResult = cmdResult.Parent as CommandResult)
            {
                switch (cmdResult.Command.Name)
                {
                    case string pcaName
                    when pcaName.Equals("public", StringComparison.OrdinalIgnoreCase):
                        return await CreatePublicClientApplication()
                            .ConfigureAwait(continueOnCapturedContext: false);
                    case string pcaName
                    when pcaName.Equals("confidentials", StringComparison.OrdinalIgnoreCase):
                        return await CreateConfidentialClientApplication()
                            .ConfigureAwait(continueOnCapturedContext: false);
                }
            }

            throw new InvalidOperationException("Unable to determine whether to construct public or confidential client application");
        }
    }
}
