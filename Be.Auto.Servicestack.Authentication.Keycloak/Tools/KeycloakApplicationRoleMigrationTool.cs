using System.Reflection;
using Be.Auto.Servicestack.Authentication.Keycloak.Extensions;
using Be.Auto.Servicestack.Authentication.Keycloak.Providers.Model;
using FS.Keycloak.RestApiClient.Api;
using FS.Keycloak.RestApiClient.Authentication.ClientFactory;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using FS.Keycloak.RestApiClient.ClientFactory;
using FS.Keycloak.RestApiClient.Model;
using ServiceStack;

namespace Be.Auto.Servicestack.Authentication.Keycloak.Tools
{
    internal class KeycloakApplicationRoleMigrationTool
    {
        private readonly KeycloakOption _options;
        internal KeycloakApplicationRoleMigrationTool(KeycloakOption options) => _options = options;
        internal void Migrate()
        {
            var source = GetRolesFromServiceTypes();
            if (!source.Any())
            {
                return;
            }

            foreach (var tuple in source.Where(t => !string.IsNullOrEmpty(t.Item1)))
            {
                CreateRoleIfNotExistAsync(new RoleRepresentation(Guid.NewGuid().ToString(), tuple.Item1, tuple.Item2));

            }
        }

        private List<Tuple<string, string>> GetRolesFromServiceTypes()
        {
            var source = new List<Tuple<string, string>>();

            foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => (assembly.FullName ?? "").StartsWith(AppDomain.CurrentDomain.FriendlyName))
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(Service).IsAssignableFrom(type)))
            {
                var description = RoleExtension.AddSpaceBetweenCamelCase(type.Name);
                var methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                    .Select(methodInfo => methodInfo.GetParameters()
                        .Select(parameterInfo => parameterInfo.ParameterType.GetCustomAttributes<RouteAttribute>())
                        .Where(routeAttributes => routeAttributes.Any())
                        .SelectMany(routeAttributes => routeAttributes)
                        .Select(routeAttribute => RoleExtension.ConvertToRoleFormat(routeAttribute.Path))
                        .Distinct()
                        .ToList())
                    .Where(list => list.Any())
                    .SelectMany(list => list)
                    .Select(role => new Tuple<string, string>(role, description))
                    .ToList();

                 if (methods.Any())
                {
                    source.AddRange(methods);
                }
            }
            return source;
        }

        private void CreateRoleIfNotExistAsync(RoleRepresentation role)
        {
            var clientCredentialsFlow = new ClientCredentialsFlow
            {
                KeycloakUrl = _options.KeycloakUrl,
                Realm = _options.AdminRealm,
                ClientId = _options.AdminClientId,
                ClientSecret = _options.AdminClientSecret,
                Scope = _options.AdminScope
            };

            using var authenticationHttpClient = AuthenticationHttpClientFactory.Create<ClientCredentialsFlow>(clientCredentialsFlow);

            using var clientsApi = ApiClientFactory.Create<ClientsApi>(authenticationHttpClient);
            var client = clientsApi.GetClients(_options.AdminRealm, null, null, null, null, null, null)
                .Find(t => t.ClientId == _options.ClientId);

            if (client == null)
            {
                throw new NullReferenceException($"{_options.ClientId} client not found!");
            }

            using var roleApi = ApiClientFactory.Create<RoleContainerApi>(authenticationHttpClient);

            if (_TryFindRole(role, roleApi, client) != null)
            {
                return;
            }

            roleApi.PostClientsRolesById(_options.ClientRealm, client.Id, role);
        }

        private RoleRepresentation? _TryFindRole(RoleRepresentation role, RoleContainerApi roleApi, ClientRepresentation client)
        {
            try
            {
                return roleApi.GetClientsRolesByIdAndRoleName(_options.ClientRealm, client.Id, role.Name);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
