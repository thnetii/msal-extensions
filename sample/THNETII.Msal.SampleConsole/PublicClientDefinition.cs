using System.CommandLine;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole
{
    public class PublicClientDefinition
        : CommandLineHostingDefinition<PublicClientApplicationAppConfigExecutor>
    {
        public PublicClientDefinition()
        {
            Command = new Command("public", "Public Client Application")
            { Handler = CommandHandler };
            Command.AddAlias("pca");
        }

        public override Command Command { get; }
    }
}
