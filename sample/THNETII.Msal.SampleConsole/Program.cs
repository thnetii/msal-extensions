using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole
{
    public static partial class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var cmdParser = new CommandLineBuilder(CreateRootCommand())
                .UseDefaults()
                .UseHost(CreateHostBuilder)
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
            services.AddHttpClient(Options.DefaultName)
            //    ;
            //services.AddHttpClient("integratedWindowsAuth")
                .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                {
                    var handler = new HttpClientHandler
                    {
                        UseDefaultCredentials = true,
                    };
                    return handler;
                })
                ;
            services.AddSingleton<ClientApplicationFactory>();
            services.AddTransient<NamedClientApplicationFactory>();

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

            services.AddDataProtection();
            services.AddSingleton(serviceProvider =>
            {
                var memCache = ActivatorUtilities.GetServiceOrCreateInstance
                    <MemoryDistributedCache>(serviceProvider);
                return ActivatorUtilities.CreateInstance
                    <MsalTokenCacheProvider>(serviceProvider, memCache);
            });
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
