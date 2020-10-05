namespace THNETII.Msal.SampleConsole
{
    public class SilentAcquireTokenOptions : AcquireTokenOptions
    {
        public string AccountIdentifier { get; set; } = null!;

        public bool? ForceRefresh { get; set; }
    }
}
