using System;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var definition = new RootCommandDefinition();

            var pcaDefinition = new PublicClientDefinition();
            definition.AddSubCommandDefinition(pcaDefinition);
            pcaDefinition.AddSubCommandDefinition(new DeviceCodeCommandDefinition());
            pcaDefinition.AddSubCommandDefinition(new IntegratedWindowsAuthenticationCommandDefinition());
            pcaDefinition.AddSubCommandDefinition(new AccountCommandDefinition<PublicClientAccountsExecutor>());
            pcaDefinition.AddSubCommandDefinition(new SilentCommandDefinition<PublicClientSilentCommandExecutor>());

            var ccaDefinition = new ConfidentialClientDefinition();
            definition.AddSubCommandDefinition(ccaDefinition);
            ccaDefinition.AddSubCommandDefinition(new AccountCommandDefinition<ConfidentialClientAccountsExecutor>());
            ccaDefinition.AddSubCommandDefinition(new SilentCommandDefinition<ConfidentialClientSilentCommandExecutor>());

            var cmdParser = new CommandLineBuilder(definition.Command)
                .UseDefaults()
                .UseHostingDefinition(definition, CreateHostBuilder)
                .Build();
            return await cmdParser.InvokeAsync(args ?? Array.Empty<string>())
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = CommandLineHost.CreateDefaultBuilder(args);

            hostBuilder.ConfigureServices(ConfigureServices);

            return hostBuilder;
        }

        internal static readonly string MsalNamespace =
            typeof(IClientApplicationBase).Namespace!;

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddHttpClient<GraphServiceClient>();

            services.AddOptions<PublicClientApplicationOptions>()
                .Configure<IConfiguration>((opts, config) =>
                    config.Bind(MsalNamespace, opts))
                .BindCommandLine()
                .PostConfigure(PostConfigureAbstractApplicationOptions)
                ;
            services.AddOptions<ConfidentialClientApplicationOptions>()
                .Configure<IConfiguration>((opts, config) =>
                    config.Bind(MsalNamespace, opts))
                .BindCommandLine()
                .PostConfigure(PostConfigureAbstractApplicationOptions)
                ;

            services.AddOptions<AcquireTokenOptions>()
                .Configure<IConfiguration>((opts, config) =>
                    config.Bind("AcquireToken", opts))
                .BindCommandLine()
                .Configure<ParseResult>(BindCommandLineScopes)
                ;
            services.AddOptions<IntegratedWindowsAuthenticationAcquireTokenOptions>()
                .Configure<IConfiguration>((opts, config) =>
                    config.Bind("AcquireToken", opts))
                .BindCommandLine()
                .Configure<ParseResult>(BindCommandLineScopes)
                ;
            services.AddOptions<SilentAcquireTokenOptions>()
                .Configure<IConfiguration>((opts, config) =>
                    config.Bind("AcquireToken", opts))
                .BindCommandLine()
                .Configure<ParseResult>(BindCommandLineScopes)
                ;

            services.AddOptions<MsalTokenCacheStorageOptions>()
                .Configure(opts => opts.ApplicationAssembly = typeof(Program).Assembly)
                .Configure<IOptions<PublicClientApplicationOptions>>(
                    (opts, pca) => opts.ClientId = pca.Value.ClientId)
                .Configure<IOptions<ConfidentialClientApplicationOptions>>(
                    (opts, cca) => opts.ClientId = cca.Value.ClientId)
                ;
            services.AddScoped<MsalTokenCacheStorageProvider>();
        }

        private static void PostConfigureAbstractApplicationOptions(
            ApplicationOptions options)
        {
            if (Assembly.GetEntryAssembly() is Assembly entryAssembly)
            {
                var entryName = entryAssembly.GetName();

                options.ClientName ??= entryName.Name;
                options.ClientVersion = entryAssembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                    .InformationalVersion ??
                    entryName.Version?.ToString();
            }
        }

        private static void BindCommandLineScopes(AcquireTokenOptions options,
            ParseResult parseResult)
        {
            var scopesOptionResult = parseResult.CommandResult.Children
                .OfType<OptionResult>()
                .FirstOrDefault(r => r.Symbol.Name == nameof(options.Scopes));

            if (scopesOptionResult?.GetValueOrDefault() is IEnumerable<string> scopes)
            {
                options.Scopes.AddRange(scopes);
            }
        }
    }
}
