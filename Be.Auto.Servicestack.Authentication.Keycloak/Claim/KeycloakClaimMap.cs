using System.Linq.Expressions;
using ServiceStack;

namespace Be.Auto.Servicestack.Authentication.Keycloak.Claim
{
  public class KeycloakClaimMap
  {
    public KeycloakClaimMap(Expression<Func<AuthUserSession, object>> property, string jsonKey)
    {
      Property = property;
      JsonKey = jsonKey;
    }

    public Expression<Func<AuthUserSession, object>> Property { get; set; }

    public string JsonKey { get; set; }
  }
}
