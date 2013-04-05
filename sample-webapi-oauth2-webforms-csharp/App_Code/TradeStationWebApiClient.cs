using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using DotNetOpenAuth.AspNet.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.Messaging;

namespace OAuth2Sample.App_Code
{
    /// <summary>
    /// The TradeStation account client.
    /// </summary>
    public class TradeStationWebApiClient : OAuth2Client
    {
        #region Constants and Fields

        /// <summary>
        /// The authorization endpoint.
        /// </summary>
        private const string AuthorizationEndpoint = "https://sim.api.tradestation.com/v2/authorize";

        /// <summary>
        /// The token endpoint.
        /// </summary>
        private const string TokenEndpoint = "https://sim.api.tradestation.com/v2/security/authorize";

        /// <summary>
        /// The _client id.
        /// </summary>
        private readonly string _clientId;

        /// <summary>
        /// The _client secret.
        /// </summary>
        private readonly string _clientSecret;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TradeStationWebApiClient"/> class.
        /// </summary>
        /// <param name="clientId">
        /// The app id.
        /// </param>
        /// <param name="clientSecret">
        /// The app secret.
        /// </param>
        public TradeStationWebApiClient(string clientId, string clientSecret)
            : this("TradeStation WebAPI", clientId, clientSecret)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TradeStationWebApiClient"/> class.
        /// </summary>
        /// <param name="providerName">The provider name.</param>
        /// <param name="clientId">The app id.</param>
        /// <param name="clientSecret">The app secret.</param>
        protected TradeStationWebApiClient(string providerName, string clientId, string clientSecret)
            : base(providerName)
        {
            if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentNullException("clientId");
            if (string.IsNullOrWhiteSpace(clientSecret)) throw new ArgumentNullException("clientSecret");

            this._clientId = clientId;
            this._clientSecret = clientSecret;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the identifier for this client as it is registered with TradeStation.
        /// </summary>
        protected string ClientId
        {
            get { return this._clientId; }
        }

        /// <summary>
        /// The Access Token for protected resources.
        /// </summary>
        protected AccessToken Token { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the full url pointing to the login page for this client. The url should include the specified return url so that when the login completes, user is redirected back to that url.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>
        /// An absolute URL.
        /// </returns>
        protected override Uri GetServiceLoginUrl(Uri returnUrl)
        {
            var builder = new UriBuilder(AuthorizationEndpoint);
            builder.AppendQueryArgument("client_id", this._clientId);
            builder.AppendQueryArgument("response_type", "code");
            builder.AppendQueryArgument("redirect_uri", returnUrl.AbsoluteUri);

            return builder.Uri;
        }

        /// <summary>
        /// Given the access token, gets the logged-in user's data. The returned dictionary must include two keys 'id', and 'username'.
        /// </summary>
        /// <param name="accessToken">
        /// The access token of the current user. 
        /// </param>
        /// <returns>
        /// A dictionary contains key-value pairs of user data 
        /// </returns>
        protected override IDictionary<string, string> GetUserData(string accessToken)
        {
            if (!string.IsNullOrWhiteSpace(accessToken) && Token != null)
            {
                return new Dictionary<string, string>()
                    {
                        {"id", Token.userid },
                        {"name", Token.userid }
                    };
            }

            return null;
        }

        /// <summary>
        /// Queries the access token from the specified authorization code.
        /// </summary>
        /// <param name="returnUrl">
        /// The return URL. 
        /// </param>
        /// <param name="authorizationCode">
        /// The authorization code. 
        /// </param>
        /// <returns>
        /// The query access token.
        /// </returns>
        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode)
        {
            if (string.IsNullOrWhiteSpace(authorizationCode)) return null;

            var entity =
                string.Format(
                    "grant_type=authorization_code&code={0}&client_id={1}&redirect_uri={2}&client_secret={3}",
                    authorizationCode,
                    this._clientId,
                    HttpUtility.UrlEncode(returnUrl.AbsoluteUri).Replace("http%3a%2f%2f", "http://"),
                    this._clientSecret);
            var byteArray = Encoding.UTF8.GetBytes(entity);

            var tokenRequest = WebRequest.Create(TokenEndpoint);
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.ContentLength = byteArray.Length;
            tokenRequest.Method = "POST";

            using (var requestStream = tokenRequest.GetRequestStream())
            {
                requestStream.Write(byteArray, 0, byteArray.Length);
            }

            var tokenResponse = (HttpWebResponse)tokenRequest.GetResponse();
            if (tokenResponse.StatusCode == HttpStatusCode.OK)
            {
                using (var responseStream = tokenResponse.GetResponseStream())
                {
                    using (var readStream = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        var serializer = new JavaScriptSerializer();
                        var tokenData = serializer.Deserialize<AccessToken>(readStream.ReadToEnd());

                        if (tokenData != null)
                        {
                            Token = tokenData;
                            return tokenData.access_token;
                        }
                    }
                }
            }

            return null;
        }

        #endregion
    }

    /// <summary>
    /// Access Token
    /// </summary>
    public class AccessToken
    {
        public string access_token { get; set; }
        public string expires_in { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }
        public string userid { get; set; }
    }
}