using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var definition = new RootCommandDefinition();
            definition.AddSubCommandDefinition(new DeviceCodeCommandDefinition());
            definition.AddSubCommandDefinition(new IntegratedWindowsAuthenticationCommandDefinition());
            definition.AddSubCommandDefinition(new SilentCommandDefinition());
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
                ;
            services.AddOptions<IntegratedWindowsAuthenticationAcquireTokenOptions>()
                .Configure<IConfiguration>((opts, config) =>
                    config.Bind("AcquireToken", opts))
                .BindCommandLine()
                ;
            services.AddOptions<SilentAcquireTokenOptions>()
                .Configure<IConfiguration>((opts, config) =>
                    config.Bind("AcquireToken", opts))
                .BindCommandLine()
                .Validate(opts =>
                {
                    if (string.IsNullOrEmpty(opts.AccountIdentifier))
                        throw new OptionsValidationException(
                            Options.DefaultName, opts.GetType(),
                            new[] { $"{nameof(opts.AccountIdentifier)} must be specified." }
                            );
                    return true;
                })
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
    }
}
