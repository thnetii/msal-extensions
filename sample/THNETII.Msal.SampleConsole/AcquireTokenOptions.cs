using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace THNETII.Msal.SampleConsole
{
    [SuppressMessage("Performance",
        "CA1819: Properties should not return arrays",
        Justification = "Options type")]
    public class AcquireTokenOptions
    {
        public string? Username { get; set; }
        public List<string> Scopes { get; } = new List<string>();
    }
}
