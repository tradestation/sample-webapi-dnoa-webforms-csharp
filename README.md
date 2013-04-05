# sample-webapi-dnoa-webforms-csharp

This sample application uses C# with .NET 4.5 to authenticate with the TradeStation API via an OAuth 2 Authorization Code Grant Type. The user will be directed to TradeStation's login page to capture credentials. After a successful login, an auth code is return and is then exchanged for an access token which will be used for subsequent WebAPI calls.

## Configuration
Modify the following fields in `App_Start\AuthConfig.cs` with your appropriate values:

    OpenAuth.AuthenticationClients.Add("TradeStation WebAPI", () => new TradeStationWebApiClient(
                    clientId: "Your API Key",
                    clientSecret: "Your API Secret"));

## Build Instructions
* Download and Extract the zip or clone this repo
* Open Visual Studio
* Build and Run

## Troubleshooting
If there are any problems, open an [issue](https://github.com/tradestation/sample-webapi-dnoa-webforms-csharp/issues) and we'll take a look! You can also give us feedback by e-mailing webapi@tradestation.com.
