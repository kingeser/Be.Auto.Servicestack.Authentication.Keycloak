using System.Reflection;
using Be.Auto.Servicestack.Authentication.Keycloak.Extensions;
using ServiceStack;
using ServiceStack.Web;

namespace Be.Auto.Servicestack.Authentication.Keycloak.Attributes
{
    public class KeycloakRequiredRoleAttribute : RequiredRoleAttribute
    {
        public override Task ExecuteAsync(IRequest req, IResponse res, object requestDto)
        {
            RequiredRoles.AddDistinctRange(requestDto.GetType().GetCustomAttributes<RouteAttribute>().Select((Func<RouteAttribute, string>)(a => RoleExtension.ConvertToRoleFormat(a.Path))).Distinct().ToList());
            return base.ExecuteAsync(req, res, requestDto);
        }

    }
}
