using System;
using System.CommandLine.Parsing;
using System.Net.Http;

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

        protected virtual void ConfigureBuilder<T>(AbstractApplicationBuilder<T> builder)
            where T : AbstractApplicationBuilder<T>
        {
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            if (!(httpClientFactory is null))
                builder.WithHttpClientFactory(httpClientFactory);
            if (!(loggerFactory is null))
                builder.WithLoggerFactory(loggerFactory);
        }

        public IPublicClientApplication CreatePublicClientApplication()
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<PublicClientApplicationOptions>>()
                .Value;
            var builder = PublicClientApplicationBuilder
                .CreateWithApplicationOptions(options);
            ConfigureBuilder(builder);
            var app = builder.Build();
            var cacheStorageProvider = serviceProvider.GetService<MsalTokenCacheProvider>();
            if (cacheStorageProvider is not null)
            {
                cacheStorageProvider.RegisterCache(app.UserTokenCache);
            }
            return app;
        }

        public IConfidentialClientApplication CreateConfidentialClientApplication()
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<ConfidentialClientApplicationOptions>>()
                .Value;
            var builder = ConfidentialClientApplicationBuilder
                .CreateWithApplicationOptions(options);
            ConfigureBuilder(builder);
            var app = builder.Build();
            var cacheStorageProvider = serviceProvider.GetService<MsalTokenCacheProvider>();
            if (cacheStorageProvider is not null)
            {
                cacheStorageProvider.RegisterCache(app.UserTokenCache);
                cacheStorageProvider.RegisterCache(app.AppTokenCache);
            }
            return app;
        }

        public IClientApplicationBase CreateClientApplication()
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
                        return CreatePublicClientApplication();
                    case string pcaName
                    when pcaName.Equals("confidentials", StringComparison.OrdinalIgnoreCase):
                        return CreateConfidentialClientApplication();
                }
            }

            throw new InvalidOperationException("Unable to determine whether to construct public or confidential client application");
        }
    }

    //public class NamedClientApplicationFactory : ClientApplicationFactory
    //{
    //    private readonly IServiceProvider serviceProvider;

    //    public NamedClientApplicationFactory(IServiceProvider serviceProvider,
    //        string? name = default) : base(serviceProvider)
    //    {
    //        this.serviceProvider = serviceProvider;
    //        Name = name;
    //    }

    //    public string? Name { get; set; }

    //    protected override void ConfigureBuilder<T>(AbstractApplicationBuilder<T> builder)
    //    {
    //        base.ConfigureBuilder(builder);
    //        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

    //        if (!(httpClientFactory is null))
    //            builder.WithHttpClientFactory(httpClientFactory, Name);
    //    }
    //}
}
