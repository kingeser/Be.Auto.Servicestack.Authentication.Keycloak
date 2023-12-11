# Be.Auto.Servicestack.Authentication.Keycloak
Servicestack Keycloak Role Based Authentication

This project demonstrates how to implement Keycloak role-based authentication in a ServiceStack application. The authentication is achieved using the `KeycloakCredentialsAuthProvider` and `KeycloakOAuthProvider` provided by the ServiceStack framework.

## Usage

```csharp
var authFeature = new AuthFeature(() => new AuthUserSession(), new IAuthProvider[]
{
    new KeycloakCredentialsAuthProvider(AppSettings)
        .MapClaim(t => t.Roles, "role")
        .MapClaim(t => t.Permissions, "groups"),
    new KeycloakOAuthProvider(AppSettings)
        .MapClaim(t => t.Roles, "role")
        .MapClaim(t => t.Permissions, "groups")
});

Plugins.Add(authFeature);

if (HostingEnvironment.IsDevelopment())
{
    authFeature.MigrateKeycloakRoles(AppSettings);
}
```


`KeycloakRequiredRole` Attribute
The KeycloakRequiredRole attribute is used to automatically validate the presence of a specific role in the request. Apply this attribute to the ServiceStack services or methods that require certain roles for access.
```csharp
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
```

Migration of Keycloak Roles
During development, you can use the MigrateKeycloakRoles method to migrate Keycloak roles to your ServiceStack application. This ensures that your roles are synchronized with the Keycloak server.
```csharp
if (HostingEnvironment.IsDevelopment())
{
    authFeature.MigrateKeycloakRoles(AppSettings);
}
```
