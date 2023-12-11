
using ServiceStack.Text;

namespace Be.Auto.Servicestack.Authentication.Keycloak.Providers.Model
{
    public class KeycloakTokenResponse
    {
    
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public int RefreshExpiresIn { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; }
        public string SessionState { get; set; }
        public string IdToken { get; set; }
        public string Scope { get; set; }
        public Dictionary<string, string> ToDictionary() => JsonObject.Parse(JsonSerializer.SerializeToString(this)).ToDictionary();
    }
}
