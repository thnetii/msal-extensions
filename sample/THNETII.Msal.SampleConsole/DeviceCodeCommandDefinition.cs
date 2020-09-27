using System.CommandLine;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole
{
    public class DeviceCodeCommandDefinition : CommandLineHostingDefinition<DeviceCodeCommandExecutor>
    {
        public DeviceCodeCommandDefinition() : base()
        {
            Command = new Command("dca", "Perform Device Code Token acquisition")
            { Handler = CommandHandler };

            ScopesArgument = new Argument<string[]>()
            {
                Name = nameof(AcquireTokenOptions.Scopes),
                Description = "Requested access scopes",
                Arity = ArgumentArity.ZeroOrMore
            };
            Command.AddArgument(ScopesArgument);
        }

        public override Command Command { get; }
        public Argument<string[]> ScopesArgument { get; }
    }
}
