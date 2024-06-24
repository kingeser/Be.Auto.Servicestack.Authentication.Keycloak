using Be.Auto.Servicestack.Authentication.Keycloak.Claim;
using ServiceStack.Auth;
using System.Linq.Expressions;
using ServiceStack;

namespace Be.Auto.Servicestack.Authentication.Keycloak.Providers;

public interface IKeycloakAuthProvider : IAuthProvider
{
    public IKeycloakAuthProvider MapClaim(KeycloakClaimMap map);
    public IKeycloakAuthProvider MapClaims(params KeycloakClaimMap[] maps);
    public IKeycloakAuthProvider MapClaim(Expression<Func<AuthUserSession, object>> property, string jsonKey);

}