using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

using THNETII.CommandLine.Hosting;
using THNETII.Msal.SampleConsole.DeviceCodeToken;

namespace THNETII.Msal.SampleConsole
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var definition = new CommandLineDefinition(typeof(Program));
            definition.AddSubCommandDefinition(new DeviceCodeDefinition());
            var cmdParser = new CommandLineBuilder(definition.Command)
                .UseDefaults()
                .UseHostWithDefinition(definition, CreateHostBuilder)
                .Build();
            return await cmdParser.InvokeAsync(args ?? Array.Empty<string>())
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        [SuppressMessage("Design",
            "CA1031: Do not catch general exception types",
            Justification = "Sample code, exceptions are logged")]
        public static void Run(IHost host)
        {
            _ = host ?? throw new ArgumentNullException(nameof(host));

            using var serviceScope = host.Services.CreateScope();
            var serviceProvider = serviceScope.ServiceProvider;

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>()
                ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(Program));

            using var httpClient = serviceProvider
                .GetRequiredService<IHttpClientFactory>().CreateClient();
            var jsonOptions = new JsonSerializerOptions { MaxDepth = 3, WriteIndented = true };
            try
            {
                var pcaOptions = serviceProvider
                    .GetRequiredService<IOptions<PublicClientApplicationOptions>>()
                    .Value;
                var pcaApp = PublicClientApplicationBuilder
                    .CreateWithApplicationOptions(pcaOptions)
                    .WithHttpClient(httpClient)
                    .WithLoggerFactory(loggerFactory)
                    .Build();

                _ = pcaApp;

                var eventId = new EventId(1, "PublicClientApplication");
                LogAppConfig(logger, eventId, pcaApp.AppConfig);
            }
            catch (Exception pcaExcept)
            {
                logger.LogError(pcaExcept, pcaExcept.Message);
            }
        }

        private static void LogAppConfig(ILogger logger, EventId eventId,
            IAppConfig appConfig)
        {
            static string GetMessage(string name) =>
                    $"{name}: {{{name}}}";

            using (logger.BeginScope(GetMessage(nameof(IClientApplicationBase.AppConfig)), appConfig))
            {
                var messageText = new StringBuilder();
                var messageArgs = new List<object?>();
                void MessageAdd(string name, object value)
                {
                    messageText.AppendLine(GetMessage(name));
                    messageArgs.Add(value);
                }

                MessageAdd(nameof(appConfig.ClientId), appConfig.ClientId);
                MessageAdd(nameof(appConfig.EnablePiiLogging), appConfig.EnablePiiLogging);
                MessageAdd(nameof(appConfig.HttpClientFactory), appConfig.HttpClientFactory);
                MessageAdd(nameof(appConfig.LogLevel), appConfig.LogLevel);
                MessageAdd(nameof(appConfig.IsDefaultPlatformLoggingEnabled), appConfig.IsDefaultPlatformLoggingEnabled);
                MessageAdd(nameof(appConfig.RedirectUri), appConfig.RedirectUri);
                MessageAdd(nameof(appConfig.TenantId), appConfig.TenantId);
                messageText.Append($"{nameof(appConfig.ExtraQueryParameters)}: {{{nameof(appConfig.ExtraQueryParameters)}}} (Count: {{{nameof(appConfig.ExtraQueryParameters) + nameof(appConfig.ExtraQueryParameters.Count)}}})");
                messageArgs.Add(appConfig.ExtraQueryParameters.ToString());
                messageArgs.Add(appConfig.ExtraQueryParameters.Count);
                foreach (var extraQueryParam in appConfig.ExtraQueryParameters)
                {
                    messageText.AppendLine().Append("- ");
                    var name = nameof(appConfig.ExtraQueryParameters) + "_" +
                        extraQueryParam.Key;
                    messageText.Append(extraQueryParam.Key).Append(": {")
                        .Append(name).Append('}');
                    messageArgs.Add(extraQueryParam.Value);
                }
                messageText.AppendLine();
                MessageAdd(nameof(appConfig.IsBrokerEnabled), appConfig.IsBrokerEnabled);
                MessageAdd(nameof(appConfig.ClientName), appConfig.ClientName);
                MessageAdd(nameof(appConfig.ClientVersion), appConfig.ClientVersion);
                MessageAdd(nameof(appConfig.TelemetryConfig), appConfig.TelemetryConfig);
                if (appConfig.TelemetryConfig is ITelemetryConfig telemetryConfig)
                {
                    var prefix = nameof(appConfig.TelemetryConfig) + '_';
                    messageText.AppendLine($"- {nameof(telemetryConfig.AudienceType)}: {{{prefix}{nameof(telemetryConfig.AudienceType)}}}");
                    messageArgs.Add(telemetryConfig.AudienceType);
                    messageText.AppendLine($"- {nameof(telemetryConfig.SessionId)}: {{{prefix}{nameof(telemetryConfig.SessionId)}}}");
                    messageArgs.Add(telemetryConfig.SessionId);
                }
                MessageAdd(nameof(appConfig.ExperimentalFeaturesEnabled), appConfig.ExperimentalFeaturesEnabled);
                int capIdx = 0;
                messageText.Append($"{nameof(appConfig.ClientCapabilities)}: {{{nameof(appConfig.ClientCapabilities)}}} (Count: {{{nameof(appConfig.ClientCapabilities) + nameof(Enumerable.Count)}}})");
                messageArgs.Add(appConfig.ClientCapabilities?.ToString());
                foreach (var capability in appConfig.ClientCapabilities ?? Enumerable.Empty<string>())
                {
                    messageText.AppendLine().Append("- ");
                    var name = nameof(appConfig.ClientCapabilities) + "_" +
                        capIdx.ToString(CultureInfo.InvariantCulture);
                    messageText.Append('{').Append(name).Append('}');
                    capIdx++;
                }
                messageArgs.Add(capIdx);
                messageArgs.AddRange(appConfig.ClientCapabilities ?? Enumerable.Empty<string>());
                messageText.AppendLine();
                MessageAdd(nameof(appConfig.ClientSecret), appConfig.ClientSecret);
                MessageAdd(nameof(appConfig.ClientCredentialCertificate), appConfig.ClientCredentialCertificate);
                if (appConfig.ClientCredentialCertificate is { Thumbprint: string cert })
                {
                    var prefix = nameof(appConfig.ClientCredentialCertificate) + '_';
                    messageText.AppendLine($"- {nameof(appConfig.ClientCredentialCertificate.Thumbprint)}: {{{prefix}{nameof(appConfig.ClientCredentialCertificate.Thumbprint)}}}");
                    messageArgs.Add(cert);
                }

                logger.LogInformation(eventId, messageText.ToString().Trim(),
                    messageArgs.ToArray());
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args ?? Array.Empty<string>());

            hostBuilder.ConfigureServices(ConfigureServices);
            hostBuilder.ConfigureServices(DeviceCodeExecutor.ConfigureServices);

            return hostBuilder;
        }

        internal static readonly string MsalNamespace =
            typeof(IClientApplicationBase).Namespace!;

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddSingleton(serviceProvider =>
            {
                var cmdDefinition = serviceProvider
                    .GetRequiredService<CommandLineDefinition>();
                var modelBinder = new ModelBinder<ApplicationOptions>()
                { EnforceExplicitBinding = true };

                modelBinder.BindMemberFromValue(
                    _ => _.ClientId, cmdDefinition.ClientIdOption);
                modelBinder.BindMemberFromValue(
                    _ => _.TenantId, cmdDefinition.TenantIdOption);
                modelBinder.BindMemberFromValue(
                    _ => _.Instance, cmdDefinition.InstanceOption);
                modelBinder.BindMemberFromValue(
                    _ => _.EnablePiiLogging, cmdDefinition.PiiLoggingOption);

                return modelBinder;
            });
            services.AddSingleton(serviceProvider =>
            {
                var cmdDefinition = serviceProvider
                    .GetRequiredService<CommandLineDefinition>();
                var modelBinder = new ModelBinder<ConfidentialClientApplicationOptions>()
                { EnforceExplicitBinding = true };

                modelBinder.BindMemberFromValue(
                    _ => _.ClientSecret, cmdDefinition.ClientSecretOption);

                return modelBinder;
            });

            services.AddOptions<PublicClientApplicationOptions>()
                .Configure<IConfiguration>((opts, config) =>
                    config.Bind(MsalNamespace, opts))
                .Configure<IServiceProvider>(BindApplicationOptionsCommandLine)
                .PostConfigure(PostConfigureAbstractApplicationOptions)
                ;
            services.AddOptions<ConfidentialClientApplicationOptions>()
                .Configure<IConfiguration>((opts, config) =>
                    config.Bind(MsalNamespace, opts))
                .Configure<IServiceProvider>(BindApplicationOptionsCommandLine)
                .PostConfigure(PostConfigureAbstractApplicationOptions)
                ;


        }

        private static void BindApplicationOptionsCommandLine<T>(
            T options, IServiceProvider serviceProvider)
            where T : ApplicationOptions
        {
            var bindingContext = serviceProvider
                .GetRequiredService<BindingContext>();
            serviceProvider.GetRequiredService<ModelBinder<ApplicationOptions>>()
                .UpdateInstance(options, bindingContext);
            serviceProvider.GetService<ModelBinder<T>>()?
                .UpdateInstance(options, bindingContext);
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
