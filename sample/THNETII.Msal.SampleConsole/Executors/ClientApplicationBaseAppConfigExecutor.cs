using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public class ClientApplicationBaseAppConfigExecutor
        : ClientApplicationBaseExecutor
    {
        public ClientApplicationBaseAppConfigExecutor(
            ClientApplicationFactory clientApplicationFactory,
            ILoggerFactory? loggerFactory = null)
            : base(clientApplicationFactory, loggerFactory) { }

        public override sealed Task<int> RunAsync(CancellationToken cancelToken = default)
        {
            var app = CreateClientApplication();
            LogAppConfig(app.AppConfig);

            return Task.FromResult(0);
        }

        private void LogAppConfig(IAppConfig appConfig)
        {
            static string GetMessage(string name) => $"{name}: {{{name}}}";

            using (Logger.BeginScope(GetMessage(nameof(IClientApplicationBase.AppConfig)), appConfig))
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

                Logger.LogInformation(messageText.ToString().Trim(),
                    messageArgs.ToArray());
            }
        }

    }
}
