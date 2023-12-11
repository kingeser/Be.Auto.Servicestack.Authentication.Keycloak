using Be.Auto.Servicestack.Authentication.Keycloak.Attributes;
using Example.Authentication.Keycloak.Servicestack.ServiceModel;

namespace Example.Authentication.Keycloak.Servicestack.ServiceInterface
{
    [KeycloakRequiredRole]
    public class ProductService : Service
    {
        public object Any(SaveProduct request)
        {
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
    }
}
