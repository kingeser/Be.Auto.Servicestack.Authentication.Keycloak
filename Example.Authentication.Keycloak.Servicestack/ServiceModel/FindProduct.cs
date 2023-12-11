namespace Example.Authentication.Keycloak.Servicestack.ServiceModel;

[Route("/product/find")]
[Route("/product/find/{Name}")]
public class FindProduct : IReturn<ProductResponse>
{
    public string? Name { get; set; }
}