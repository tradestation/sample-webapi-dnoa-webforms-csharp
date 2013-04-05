using Microsoft.AspNet.Membership.OpenAuth;
using OAuth2Sample.App_Code;

namespace OAuth2Sample.App_Start
{
    internal static class AuthConfig
    {
        public static void RegisterOpenAuth()
        {
            OpenAuth.AuthenticationClients.Add("TradeStation WebAPI", () => new TradeStationWebApiClient(
                    clientId: "Your API Key",
                    clientSecret: "Your API Secret"));
        }
    }
}