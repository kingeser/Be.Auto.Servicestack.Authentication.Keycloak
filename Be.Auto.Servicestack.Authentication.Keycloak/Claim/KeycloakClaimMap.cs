using System.Linq.Expressions;
using Be.Auto.Servicestack.Authentication.Keycloak.Extensions;
using ServiceStack;
using ServiceStack.Auth;

namespace Be.Auto.Servicestack.Authentication.Keycloak.Claim
{

    public partial class KeycloakClaimMap
    {
        public KeycloakClaimMap(Expression<Func<IAuthSessionExtended, object>> property, string jsonKey)
        {
            Property = property.PropertyName();
            JsonKey = jsonKey;
        }

        public KeycloakClaimMap(string property, string jsonKey)
        {
            Property = property;
            JsonKey = jsonKey;
        }

        public string Property { get; set; }

        public string JsonKey { get; set; }
    }
}
