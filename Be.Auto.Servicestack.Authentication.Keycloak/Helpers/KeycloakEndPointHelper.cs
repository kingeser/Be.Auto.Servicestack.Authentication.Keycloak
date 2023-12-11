
using Be.Auto.Servicestack.Authentication.Keycloak.Extensions;
using Be.Auto.Servicestack.Authentication.Keycloak.Providers.Model;
using ServiceStack;
using ServiceStack.Text;

namespace Be.Auto.Servicestack.Authentication.Keycloak.Helpers
{
    internal static class KeycloakEndPointHelper
    {
        internal static KeycloakDocumentDiscovery FindEndPoints(string authRealm)
        {
            var jsonObject = JsonObject.Parse((authRealm + "/.well-known/openid-configuration").NormalizeUrl().GetJsonFromUrl());
            return new KeycloakDocumentDiscovery
            {
                AuthorizeUrl = jsonObject.Get("authorization_endpoint"),
                UserInfoUrl = jsonObject.Get("userinfo_endpoint"),
                TokenUrl = jsonObject.Get("token_endpoint"),
                IntrospectUrl = jsonObject.Get("introspection_endpoint"),
                JwksUrl = jsonObject.Get("jwks_uri")
            };
        }
        internal static async Task<KeycloakDocumentDiscovery> FindEndPointsAsync(string authRealm)
        {
            var jsonObject = JsonObject.Parse(await (authRealm + "/.well-known/openid-configuration").NormalizeUrl().GetJsonFromUrlAsync());
            return new KeycloakDocumentDiscovery
            {
                AuthorizeUrl = jsonObject.Get("authorization_endpoint"),
                UserInfoUrl = jsonObject.Get("userinfo_endpoint"),
                TokenUrl = jsonObject.Get("token_endpoint"),
                IntrospectUrl = jsonObject.Get("introspection_endpoint"),
                JwksUrl = jsonObject.Get("jwks_uri")
            };
        }
    }
}
