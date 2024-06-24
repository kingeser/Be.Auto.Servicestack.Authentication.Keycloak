using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Be.Auto.Servicestack.Authentication.Keycloak.Claim;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Text;

namespace Be.Auto.Servicestack.Authentication.Keycloak.Extensions
{
    internal static class ClaimExtensions
    {
        internal static string? PropertyName(this Expression<Func<AuthUserSession, object>>? func) =>
            func?.Body switch
            {
                MemberExpression memberExpression => memberExpression.Member.Name,
                UnaryExpression { Operand: MemberExpression operand } => operand.Member.Name,
                _ => func?.ToString().Split('.').Last()
            };
        internal static string? PropertyName(this Expression<Func<IAuthSessionExtended, object>>? func) =>
            func?.Body switch
            {
                MemberExpression memberExpression => memberExpression.Member.Name,
                UnaryExpression { Operand: MemberExpression operand } => operand.Member.Name,
                _ => func?.ToString().Split('.').Last()
            };

        internal static string? PropertyName(this Expression<Func<IAuthSession, object>>? func) =>
            func?.Body switch
            {
                MemberExpression memberExpression => memberExpression.Member.Name,
                UnaryExpression { Operand: MemberExpression operand } => operand.Member.Name,
                _ => func?.ToString().Split('.').Last()
            };

        internal static string? PropertyName(this Expression<Func<object, object>>? func) =>
            func?.Body switch
            {
                MemberExpression memberExpression => memberExpression.Member.Name,
                UnaryExpression { Operand: MemberExpression operand } => operand.Member.Name,
                _ => func?.ToString().Split('.').Last()
            };
        private static bool IsPropertyDictionary(Type? propertyType) => propertyType != null && (typeof(IDictionary).IsAssignableFrom(propertyType) || propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IDictionary<object, object>)) && propertyType != typeof(string);
        private static bool IsPropertyEnumerable(Type? propertyType) => propertyType != null && (typeof(IEnumerable).IsAssignableFrom(propertyType) || propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) && propertyType != typeof(string);

        internal static void AddMap(this ICollection<KeycloakClaimMap> list, KeycloakClaimMap item)

        {

            list.Add(item);
        }

        internal static void AddMap(
          this ICollection<KeycloakClaimMap> list,
          ICollection<KeycloakClaimMap> items)
        {
            foreach (var keycloakClaimMap in items)
            {
                list.AddMap(keycloakClaimMap);
            }
        }

        internal static async Task MapClaims(
          this IEnumerable<KeycloakClaimMap> maps,
          string userProfileUrl,
          string accessToken,
          IAuthSession session,
          IAuthTokens? tokens = null)
        {
            var avaibleMaps = maps.ToList();


           avaibleMaps.AddMap(new KeycloakClaimMap(t => t.UserAuthName, "preferred_username"));
           avaibleMaps.AddMap(new KeycloakClaimMap(t => t.UserAuthId, "sub"));
           avaibleMaps.AddMap(new KeycloakClaimMap(t => t.EmailConfirmed!, "email_verified"));
           avaibleMaps.AddMap(new KeycloakClaimMap(t => t.Roles, "role"));
           avaibleMaps.AddMap(new KeycloakClaimMap(t => t.Permissions, "groups"));
           avaibleMaps.AddMap(new KeycloakClaimMap(t => t.DisplayName, "name"));
           avaibleMaps.AddMap(new KeycloakClaimMap(t => t.UserName, "preferred_username"));
           avaibleMaps.AddMap(new KeycloakClaimMap(t => t.FirstName, "given_name"));
           avaibleMaps.AddMap(new KeycloakClaimMap(t => t.LastName, "family_name"));
           avaibleMaps.AddMap(new KeycloakClaimMap(t => t.Email, "email"));

            avaibleMaps = avaibleMaps.GroupBy(t => new { t.Property, t.JsonKey }).Select(t => t.First()).ToList();

            var json = await userProfileUrl.PostStringToUrlAsync(requestFilter: req => req.With(c =>
            {
                c.UserAgent = ServiceClientBase.DefaultUserAgent;
                c.Authorization = new NameValue("Bearer", accessToken);
            }));

            var obj = JsonObject.Parse(json);

            foreach (var keycloakClaimMap in avaibleMaps)
            {
                var property = keycloakClaimMap.Property;

                var jsonKey = keycloakClaimMap.JsonKey;

                var sessionProp = session.GetType().GetProperty(property);

                if (sessionProp != null)
                {
                    TryMapProperty(session, sessionProp, obj, jsonKey);
                }

                if (tokens == null)
                {
                    continue;
                }

                var tokensProp = tokens.GetType().GetProperty(property);

                if (tokensProp != null)
                {
                    TryMapProperty(tokens, tokensProp, obj, jsonKey);
                }


            }

        }

        private static bool IsNullableType(Type type) => Nullable.GetUnderlyingType(type) != null;

        private static void TryMapProperty(
          object session,
          PropertyInfo property,
          JsonObject obj,
          string jsonKey)
        {
            try
            {
                var propType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                var converter = TypeDescriptor.GetConverter(propType);
                var text = obj.Get(jsonKey);
                if (IsPropertyDictionary(propType))
                {
                    var dictionary = property.GetValue(session)?.ConvertTo<IDictionary<object, object>>() ?? Activator.CreateInstance<Dictionary<object, object>>();

                    dictionary.TryGetValue(jsonKey, out var outputValue);

                    var existValue = outputValue?.ToString()?.Split("|")?.Where(t => !string.IsNullOrEmpty(t)).ConvertTo<List<object>>() ?? [];

                    var enumerableValue = JsonSerializer.DeserializeFromString<IEnumerable<object>>(obj.GetArray<object>(jsonKey).SerializeToString());

                    var newValue = existValue?.Union(enumerableValue)?.Distinct()?.Where(t => !string.IsNullOrEmpty(t?.ToString()))!.Join("|") ?? string.Empty;

                    dictionary[jsonKey] = newValue;

                    property.SetValue(session, dictionary.ConvertTo(property.PropertyType), null);
                }
                else if (IsPropertyEnumerable(propType))
                {

                    var existValue = property.GetValue(session)?.ConvertTo<IEnumerable<object>>() ?? Activator.CreateInstance<List<object>>();

                    var enumerableValue = JsonSerializer.DeserializeFromString<IEnumerable<object>>(obj.GetArray<object>(jsonKey).SerializeToString());

                    var newValue = existValue.Union(enumerableValue).ConvertTo(propType);

                    property.SetValue(session, newValue, null);

                }


                else
                {
                    var value = converter.ConvertFromString(text);
                    property.SetValue(session, value, null);
                }
            }
            catch (Exception e)
            {
                var message = $"Keycloak Claim cannot be mapped: Property = {property.Name} ({property.PropertyType.Name}), Json Key: {jsonKey}";
                HostContext.TryResolve<ILogger>()?.LogWarning(e, message);

            }
        }


    }
}
