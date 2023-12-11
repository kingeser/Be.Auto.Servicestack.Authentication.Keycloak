
namespace Be.Auto.Servicestack.Authentication.Keycloak.Providers.Model
{
    public class KeycloakOption
    {
        public required string KeycloakUrl { get; set; }
        public required string AdminRealm { get; set; }
        public required string AdminClientId { get; set; }
        public required string AdminClientSecret { get; set; }
        public string? AdminScope { get; set; }
        public required string ClientId { get; set; }
        public required string ClientRealm { get; set; }

    }
}
