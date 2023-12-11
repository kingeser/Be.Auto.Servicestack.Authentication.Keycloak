namespace Example.Authentication.Keycloak.Servicestack.ServiceModel;

public class GetIdentityResponse
{
    public List<Property> Claims { get; set; }
    public AuthUserSession Session { get; set; }
 
}