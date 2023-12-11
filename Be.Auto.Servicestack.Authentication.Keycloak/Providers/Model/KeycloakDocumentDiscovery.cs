
namespace Be.Auto.Servicestack.Authentication.Keycloak.Providers.Model
{
    public class KeycloakDocumentDiscovery
    {
        public string AuthorizeUrl { get; set; }
        public string IntrospectUrl { get; set; }
        public string UserInfoUrl { get; set; }
        public string TokenUrl { get; set; }
        public string JwksUrl { get; set; }
    }
}
