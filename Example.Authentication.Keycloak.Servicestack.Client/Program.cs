
using System.Text.Json;
using Be.Auto.Servicestack.Authentication.Keycloak.Providers;
using Example.Authentication.Keycloak.Servicestack.ServiceModel;
using ServiceStack;
using JsonSerializer = System.Text.Json.JsonSerializer;

Console.WriteLine("Servicestack Client Demo");

Console.WriteLine("---------------------------------------------------------");

var client = new JsonServiceClient("https://localhost:44312")
{
    UserName = "test@test.com",
    Password = "123456",
    AlwaysSendBasicAuthHeader = true

};

try
{
    var authenticateResponse = client.Post(new Authenticate(KeycloakAuthProviders.CredentialsProvider)
    {
        UserName = "test@test.com",
        Password = "123456",
        RememberMe = true
    });

    Console.WriteLine("Authenticate Success : " + Environment.NewLine + JsonSerializer.Serialize(authenticateResponse,
        new JsonSerializerOptions()
        {
            WriteIndented = true
        }));
}
catch (WebServiceException e)
{
    Console.WriteLine($"Authenticate Error : {e.StatusDescription} > {e.Message}");
}

Console.WriteLine("---------------------------------------------------------");

try
{
    var saveProductResponse = await client.PostAsync(new SaveProduct()
    {
        Name = "Test"
    });

    Console.WriteLine("Save Product Success : " + Environment.NewLine + JsonSerializer.Serialize(saveProductResponse, new JsonSerializerOptions()
    {
        WriteIndented = true
    }));

}
catch (WebServiceException e)
{
    Console.WriteLine($"Save Product Error : {e.StatusDescription} > {e.Message}");

}

Console.WriteLine("---------------------------------------------------------");

try
{
    var findProductResponse = await client.PostAsync(new FindProduct()
    {
        Name = "Test"
    });
    Console.WriteLine("Find Product Success : " + Environment.NewLine + JsonSerializer.Serialize(findProductResponse, new JsonSerializerOptions()
    {
        WriteIndented = true
    }));

}
catch (WebServiceException e)
{
    Console.WriteLine($"Find Product Error : {e.StatusDescription} > {e.Message}");
}

Console.ReadLine();