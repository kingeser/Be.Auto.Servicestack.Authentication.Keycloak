namespace Example.Authentication.Keycloak.Servicestack.ServiceModel;

[Route("/product/delete")]
[Route("/product/delete/{id}")]
public class DeleteProduct : IReturn<ProductResponse>
{
    public string? Id { get; set; }
}