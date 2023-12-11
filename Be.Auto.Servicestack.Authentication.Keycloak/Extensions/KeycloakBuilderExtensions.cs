
using Be.Auto.Servicestack.Authentication.Keycloak.Providers;
using Be.Auto.Servicestack.Authentication.Keycloak.Providers.Model;
using Be.Auto.Servicestack.Authentication.Keycloak.Tools;
using ServiceStack;
using ServiceStack.Configuration;

namespace Be.Auto.Servicestack.Authentication.Keycloak.Extensions
{
    public static class KeycloakBuilderExtensions
    {
   
        public static T MigrateKeycloakRoles<T>(this T builder, Action<KeycloakOption> opt) where T : AuthFeature
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (opt == null)
            {
                throw new ArgumentNullException(nameof(opt));
            }

            var options = new KeycloakOption
            {
                ClientId = string.Empty,
                AdminRealm = string.Empty,
                ClientRealm = string.Empty,
                AdminClientId = string.Empty,
                AdminClientSecret = string.Empty,
                KeycloakUrl = string.Empty,
                AdminScope = string.Empty
            };
            opt(options);
            TryMigrate(options);
            return builder;
        }

        public static T MigrateKeycloakRoles<T>(this T builder, IAppSettings settings) where T : AuthFeature
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            TryMigrate(new KeycloakOption
            {
                ClientId = settings.GetString("Keycloak:RoleMigration:ClientId"),
                AdminRealm = settings.GetString("Keycloak:RoleMigration:AdminRealm"),
                ClientRealm = settings.GetString("Keycloak:RoleMigration:ClientRealm"),
                AdminClientId = settings.GetString("Keycloak:RoleMigration:AdminClientId"),
                AdminClientSecret = settings.GetString("Keycloak:RoleMigration:AdminClientSecret"),
                KeycloakUrl = settings.GetString("Keycloak:RoleMigration:KeycloakUrl"),
                AdminScope = settings.GetString("Keycloak:RoleMigration:AdminScope")
            });
            return builder;
        }

        private static void TryMigrate(KeycloakOption options)
        {
            if (string.IsNullOrEmpty(options.KeycloakUrl))
            {
                throw new ArgumentNullException("KeycloakUrl");
            }

            if (string.IsNullOrEmpty(options.AdminRealm))
            {
                throw new ArgumentNullException("AdminRealm");
            }

            if (string.IsNullOrEmpty(options.AdminClientId))
            {
                throw new ArgumentNullException("AdminClientId");
            }

            if (string.IsNullOrEmpty(options.AdminClientSecret))
            {
                throw new ArgumentNullException("AdminClientSecret");
            }

            if (string.IsNullOrEmpty(options.ClientId))
            {
                throw new ArgumentNullException("ClientId");
            }

            if (string.IsNullOrEmpty(options.ClientRealm))
            {
                throw new ArgumentNullException("ClientRealm");
            }

            new KeycloakApplicationRoleMigrationTool(options).Migrate();
        }
    }
}
