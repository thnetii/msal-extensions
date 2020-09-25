using System;
using System.Diagnostics.CodeAnalysis;

namespace THNETII.Msal.SampleConsole.DeviceCodeToken
{
    [SuppressMessage("Performance",
        "CA1819: Properties should not return arrays",
        Justification = "Options type")]
    public class DeviceCodeOptions
    {
        public string[] Scopes { get; set; } = Array.Empty<string>();
    }
}
