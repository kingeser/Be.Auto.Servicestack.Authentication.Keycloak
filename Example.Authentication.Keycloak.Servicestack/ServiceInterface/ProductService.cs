using Be.Auto.Servicestack.Authentication.Keycloak.Attributes;
using Example.Authentication.Keycloak.Servicestack.ServiceModel;

namespace Example.Authentication.Keycloak.Servicestack.ServiceInterface
{
    [KeycloakRequiredRole]

    public class ProductService : Service
    {
        public object Any(SaveProduct request)
        {
            var session = SessionAs<AuthUserSession>();
            var agency = session.Meta["Agency"];
            return new ProductResponse
            {
                Result = request.Name
            };
        }
        public object Any(FindProduct request)
        {
            return new ProductResponse
            {
                Result = request.Name
            };
        }

        public object Any(DeleteProduct request)
        {
            return new ProductResponse
            {
                Result = request.Id
            };
        }
    }
}
