using Be.Auto.Servicestack.Authentication.Keycloak.Extensions;
using Be.Auto.Servicestack.Authentication.Keycloak.Providers;
using Funq;
using ServiceStack.Auth;
using Example.Authentication.Keycloak.Servicestack.ServiceInterface;

[assembly: HostingStartup(typeof(AppHost))]

namespace Example.Authentication.Keycloak.Servicestack;

public class AppHost : AppHostBase, IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices(services =>
        {




        });

    public AppHost() : base("Example.Authentication.Keycloak.Servicestack", typeof(ProductService).Assembly)
    {


    }

    public override void Configure(Container container)
    {
        SetConfig(new HostConfig
        {
            AddRedirectParamsToQueryString = true,
        });

        var authFeauture = new AuthFeature(() => new AuthUserSession(), new IAuthProvider[]
        {
            new KeycloakCredentialsAuthProvider(AppSettings)
                .MapClaim(t => t.Roles, "role")
                .MapClaim(t => t.Permissions, "groups"),
            new KeycloakOAuthProvider(AppSettings)
                .MapClaim(t => t.Roles, "role")
                .MapClaim(t => t.Permissions, "groups")
        });

        Plugins.Add(authFeauture);

        if (HostingEnvironment.IsDevelopment())
        {
            authFeauture.MigrateKeycloakRoles(AppSettings);
        }

        Plugins.Add(new SharpPagesFeature
        {
            EnableSpaFallback = true
        });

    }

}

