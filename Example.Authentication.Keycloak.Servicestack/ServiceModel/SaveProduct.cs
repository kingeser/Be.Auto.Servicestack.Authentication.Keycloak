namespace Example.Authentication.Keycloak.Servicestack.ServiceModel;

[Route("/product/save")]
public class SaveProduct : IReturn<ProductResponse>
{
    public string? Name { get; set; }
}