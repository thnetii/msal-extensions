using System.Collections.Generic;

namespace THNETII.Msal.SampleConsole
{
    public class AcquireTokenOptions
    {
        public List<string> Scopes { get; } = new List<string>();
    }
}
