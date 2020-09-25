using System;
using System.CommandLine;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole.DeviceCodeToken
{
    public class DeviceCodeDefinition : CommandLineHostDefinition
    {
        public DeviceCodeDefinition() : base(typeof(DeviceCodeExecutor))
        {
            Command = new Command("deviceCode", "Perform Device Code Token acquisition")
            { Handler = GetCommandHandler(typeof(DeviceCodeExecutor)) };

            ScopesArgument = new Argument<string[]>("SCOPES", () => Array.Empty<string>())
            {
                Description = "Requested access scopes",
                Arity = ArgumentArity.ZeroOrMore
            };
            Command.AddAlias("dc");
            Command.AddArgument(ScopesArgument);
        }

        public override Command Command { get; }
        public Argument<string[]> ScopesArgument { get; }
    }
}
