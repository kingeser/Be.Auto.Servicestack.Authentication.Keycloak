
using Be.Auto.Servicestack.Authentication.Keycloak.Providers.Model;
using ServiceStack;
using ServiceStack.Html;
using ServiceStack.Text;

namespace Be.Auto.Servicestack.Authentication.Keycloak.Helpers
{
    internal static class AccessTokenHelper
    {


        internal static async Task<KeycloakTokenResponse> RequestAccessTokenAsync(this string url, object formData, CancellationToken cancelToken = default(CancellationToken))
        {
            var response = await url.PostToUrlAsync(formData, token: cancelToken);

            var token = JsonObject.Parse(response);

            return new KeycloakTokenResponse()
            {
                AccessToken = token.Get("access_token"),
                RefreshExpiresIn = token.Get<int>("refresh_expires_in"),
                ExpiresIn = token.Get<int>("expires_in"),
                RefreshToken = token.Get("refresh_token"),
                TokenType = token.Get("token_type"),
                IdToken = token.Get("id_token"),
                SessionState = token.Get("session_state"),
                Scope = token.Get("scope"),
            };
        }


        internal static KeycloakTokenResponse RequestAccessToken(this string url, object formData)
        {
            var response = url.PostToUrl(formData);

            var token = JsonObject.Parse(response);

            return new KeycloakTokenResponse()
            {
                AccessToken = token.Get("access_token"),
                RefreshExpiresIn = token.Get<int>("refresh_expires_in"),
                ExpiresIn = token.Get<int>("expires_in"),
                RefreshToken = token.Get("refresh_token"),
                TokenType = token.Get("token_type"),
                IdToken = token.Get("id_token"),
                SessionState = token.Get("session_state"),
                Scope = token.Get("scope"),
            };
        }

        internal static async Task<bool> CheckAccessTokenIsValidAsync(
         this string verifyTokenUrl,
          string clientId,
          string clientSecret,
          Authenticate request,
          CancellationToken token)
        {
            var validTokenResultString = await verifyTokenUrl.PostToUrlAsync(new
            {
                token = request.AccessToken,
                client_id = clientId,
                client_secret = clientSecret
            }, token: token);
            var tokenJsonVar = JsonObject.Parse(validTokenResultString);
            var flag = tokenJsonVar.Get("active", false);
            return flag;
        }
        internal static bool CheckAccessTokenIsValid(
            this string verifyTokenUrl,
            string clientId,
            string clientSecret,
            string accessToken)
        {
            var validTokenResultString = verifyTokenUrl.PostToUrl(new
            {
                token = accessToken,
                client_id = clientId,
                client_secret = clientSecret
            });
            var tokenJsonVar = JsonObject.Parse(validTokenResultString);
            var flag = tokenJsonVar.Get("active", false);
            return flag;
        }
    }
}
