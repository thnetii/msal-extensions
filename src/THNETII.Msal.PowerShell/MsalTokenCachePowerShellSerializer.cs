using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

using Microsoft.Identity.Client;

namespace THNETII.Msal.PowerShell
{
    public class MsalTokenCachePowerShellSerializer
    {
        private readonly IClientApplicationBase msalApplication;
        private readonly List<PSObject> profiles;

        public MsalTokenCachePowerShellSerializer(
            IClientApplicationBase msalApplication,
            List<PSObject> profiles
            ) : base()
        {
            this.msalApplication = msalApplication
                ?? throw new ArgumentNullException(nameof(msalApplication));
            this.profiles = profiles
                ?? throw new ArgumentNullException(nameof(profiles));
        }

        public void RegisterCallbacks(ITokenCache cache)
        {
            _ = cache ?? throw new ArgumentNullException(nameof(cache));
            cache.SetBeforeAccess(OnSetBeforeAccess);
            cache.SetAfterAccess(OnSetAfterAccess);
        }

        private void OnSetBeforeAccess(TokenCacheNotificationArgs args)
        {
            var cacheProp = GetProfileCacheProperty(args, out _);
            var cacheBase64 = cacheProp == null ? null : cacheProp.Value as string;
            if (cacheBase64 != null)
            {
                byte[] cacheData = Convert.FromBase64String(cacheBase64);
                args.TokenCache.DeserializeMsalV3(cacheData);
            }
        }

        private void OnSetAfterAccess(TokenCacheNotificationArgs args)
        {
            var cacheProp = GetProfileCacheProperty(args, out var profile);
            if (profile == null)
            {
                profiles.Add(new PSObject
                {
                    Properties =
                    {
                        new PSNoteProperty("authority", msalApplication.Authority),
                        new PSNoteProperty("clientId", args.ClientId),
                        new PSNoteProperty(
                            GetCachePropertyName(args),
                            Convert.ToBase64String(args.TokenCache.SerializeMsalV3())
                            ),
                    }
                });
            }
            else if (cacheProp == null)
            {
                profile.Properties.Add(new PSNoteProperty(
                    GetCachePropertyName(args),
                    Convert.ToBase64String(args.TokenCache.SerializeMsalV3())
                    ));
            }
            else
            {
                cacheProp.Value = Convert.ToBase64String(args.TokenCache.SerializeMsalV3());
            }
        }

        private static string GetCachePropertyName(TokenCacheNotificationArgs args)
        {
            return args.IsApplicationCache ? "appOnlyCache" : "userCache";
        }

        private PSPropertyInfo? GetProfileCacheProperty(TokenCacheNotificationArgs args, out PSObject profile)
        {
            var authority = msalApplication.Authority;
            var clientId = args.ClientId;
            profile = profiles.FirstOrDefault(p =>
                IsPSObjectPropertyEqualTo(authority, p, "authority") &&
                IsPSObjectPropertyEqualTo(clientId, p, "clientId")
                );
            var cacheProperty = GetCachePropertyName(args);
            return profile?.Properties[cacheProperty];
        }

        private static bool IsPSObjectPropertyEqualTo(string value, PSObject psObject, string propertyName)
        {
            var prop = psObject.Properties[propertyName];
            string? matchValue = prop?.Value as string;
            if (string.IsNullOrEmpty(matchValue))
                return false;
            return matchValue!.Equals(value, StringComparison.OrdinalIgnoreCase);
        }
    }
}
