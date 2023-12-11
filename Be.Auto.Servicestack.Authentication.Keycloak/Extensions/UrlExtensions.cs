
namespace Be.Auto.Servicestack.Authentication.Keycloak.Extensions
{
    internal static class UrlExtensions
    {
        internal static string NormalizeUrl(this string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var result))
            {
                return url;
            }

            var uriBuilder = new UriBuilder(result);

            uriBuilder.Path = uriBuilder.Path.TrimEnd('/').Replace("//", "/");

            return uriBuilder.Uri.ToString();
        }
    }
}
