using System.CommandLine;

using Microsoft.Identity.Client;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole
{
    public class RootCommandDefinition : CommandLineGroupHostingDefinition
    {
        public RootCommandDefinition() : base()
        {
            Command = new RootCommand(GetAssemblyDescription(typeof(Program)));

            ClientIdOption = new Option<string>("--clientid")
            {
                Name = nameof(ApplicationOptions.ClientId),
                Description = "Client ID",
                Argument = { Name = "CLIENTID" }
            };
            ClientIdOption.AddAlias("-c");
            Command.AddOption(ClientIdOption);

            TenantIdOption = new Option<string>("--tenant")
            {
                Name = nameof(ApplicationOptions.TenantId),
                Description = "Tenant ID (aka. Realm)",
                Argument = { Name = "TENANT" }
            };
            TenantIdOption.AddAlias("-t");
            Command.AddOption(TenantIdOption);

            InstanceOption = new Option<string>("--instance")
            {
                Name = nameof(ApplicationOptions.Instance),
                Description = "Azure AD instance URL",
                Argument = { Name = "INSTANCE" }
            };
            Command.AddOption(InstanceOption);

            PiiLoggingOption = new Option<bool>("--pii-logging")
            {
                Name = nameof(ApplicationOptions.EnablePiiLogging),
                Description = "Enable loggig of Personal Identifiable Information",
            };
            Command.AddOption(PiiLoggingOption);
        }

        public override Command Command { get; }
        public Option<string> ClientIdOption { get; }
        public Option<string> TenantIdOption { get; }
        public Option<string> InstanceOption { get; }
        public Option<bool> PiiLoggingOption { get; }
    }
}
