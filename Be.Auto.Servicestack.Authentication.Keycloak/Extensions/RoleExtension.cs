
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Be.Auto.Servicestack.Authentication.Keycloak.Extensions
{
    internal static class RoleExtension
    {
        internal static string AddSpaceBetweenCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            var stringBuilder = new StringBuilder(input.Length * 2);
            stringBuilder.Append(input[0]);
            for (var index = 1; index < input.Length; ++index)
            {
                if (char.IsUpper(input[index]) && !char.IsWhiteSpace(input[index - 1]))
                {
                    stringBuilder.Append(' ');
                }

                stringBuilder.Append(input[index]);
            }
            return stringBuilder.ToString();
        }

        internal static string ConvertToRoleFormat(string url)
        {

            if (string.IsNullOrEmpty(url)) return url;
            
            url = Regex.Replace(url, @"/\{[^\}]+\}", "");

            var list = url.Split('/').Where(t => !string.IsNullOrEmpty(t)).ToList();

            for (var index = 0; index < list.Count; ++index)
            {
                if (!string.IsNullOrEmpty(list[index]))
                {
                    list[index] = ToTitleCase(list[index]);
                }
            }

            return string.Join(".", list.Distinct());

        }
        internal static string ToTitleCase(string input) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
    }
}
