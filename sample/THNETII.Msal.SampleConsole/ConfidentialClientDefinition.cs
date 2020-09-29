using System.CommandLine;

using Microsoft.Identity.Client;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole
{
    public class ConfidentialClientDefinition
        : CommandLineHostingDefinition<ConfidentialClientApplicationAppConfigExecutor>
    {
        public ConfidentialClientDefinition()
        {
            Command = new Command("confidential", "Confidential Client Application")
            { Handler = CommandHandler };
            Command.AddAlias("cca");

            ClientSecretOption = new Option<string>("--secret")
            {
                Name = nameof(ConfidentialClientApplicationOptions.ClientSecret),
                Description = "Client Secret",
                Argument = { Name = "SECRET" }
            };
            ClientSecretOption.AddAlias("-s");
            Command.AddOption(ClientSecretOption);
        }

        public override Command Command { get; }
        public Option<string> ClientSecretOption { get; }
    }
}
