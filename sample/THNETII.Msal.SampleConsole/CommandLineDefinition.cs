using System;
using System.CommandLine;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole
{
    public class CommandLineDefinition : CommandLineHostDefinition
    {
        public CommandLineDefinition(Type executorType) : base(executorType)
        {
            Command = new RootCommand(GetAssemblyDescription(executorType))
            { Handler = GetCommandHandler(executorType) };

            ClientIdOption = new Option<string>("--clientid")
            {
                Description = "Client ID",
                Argument = { Name = "CLIENTID" }
            };
            ClientIdOption.AddAlias("-c");
            Command.AddOption(ClientIdOption);

            ClientSecretOption = new Option<string>("--secret")
            {
                Description = "Client Secret (Confidential Apps only)",
                Argument = { Name = "SECRET" }
            };
            ClientSecretOption.AddAlias("-s");
            Command.AddOption(ClientSecretOption);

            TenantIdOption = new Option<string>("--tenant")
            {
                Description = "Tenant ID (aka. Realm)",
                Argument = { Name = "TENANT" }
            };
            TenantIdOption.AddAlias("-t");
            Command.AddOption(TenantIdOption);

            InstanceOption = new Option<string>("--instance")
            {
                Description = "Azure AD instance URL",
                Argument = { Name = "INSTANCE" }
            };
            Command.AddOption(InstanceOption);

            PiiLoggingOption = new Option<bool>("--pii-logging")
            {
                Description = "Enable loggig of Personal Identifiable Information",
            };
            Command.AddOption(PiiLoggingOption);
        }

        public override Command Command { get; }
        public Option<string> ClientIdOption { get; }
        public Option<string> ClientSecretOption { get; }
        public Option<string> TenantIdOption { get; }
        public Option<string> InstanceOption { get; }
        public Option<bool> PiiLoggingOption { get; }
    }
}
