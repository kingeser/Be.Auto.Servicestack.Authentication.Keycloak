namespace Example.Authentication.Keycloak.Servicestack.ServiceModel;

[Route("/servicestack-identity")]
public class GetIdentity : IReturn<GetIdentityResponse> { }