using Example.Authentication.Keycloak.Servicestack.ServiceModel;
using ServiceStack.Auth;

namespace Example.Authentication.Keycloak.Servicestack.ServiceInterface;

public class IdentityService : Service
{
    [Authenticate]
    public async Task<IAuthSession> Any(GetIdentity request)
    {
        return await GetSessionAsync();
    }
}