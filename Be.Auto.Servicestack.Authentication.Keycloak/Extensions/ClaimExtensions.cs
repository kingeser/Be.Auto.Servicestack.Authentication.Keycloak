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
        internal static string PropertyName(this Expression<Func<IAuthSessionExtended, object>>? func)

        {

            switch (func.Body)
            {
                case MemberExpression memberExpression:
                    return memberExpression.Member.Name;
                case UnaryExpression { Operand: MemberExpression operand }:
                    return operand.Member.Name;
                default:
                    return func.ToString().Split('.').Last();

            }



        }
        internal static string PropertyName(this Expression<Func<IAuthSession, object>>? func)

        {

            switch (func.Body)
            {
                case MemberExpression memberExpression:
                    return memberExpression.Member.Name;
                case UnaryExpression { Operand: MemberExpression operand }:
                    return operand.Member.Name;
                default:
                    return func.ToString().Split('.').Last();

            }



        }
        internal static string PropertyName(this Expression<Func<object, object>>? func)

        {

            switch (func.Body)
            {
                case MemberExpression memberExpression:
                    return memberExpression.Member.Name;
                case UnaryExpression { Operand: MemberExpression operand }:
                    return operand.Member.Name;
                default:
                    return func.ToString().Split('.').Last();

            }


        }

        private static bool IsPropertyEnumerable(Type? propertyType) => (typeof(IEnumerable).IsAssignableFrom(propertyType) || propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) && propertyType != typeof(string);

        internal static void AddMapIfNotExists(this ICollection<KeycloakClaimMap> list, KeycloakClaimMap item)

        {
            if (list.Any(x => x.Property == item.Property))
            {
                return;
            }
            list.Add(item);
        }

        internal static void AddMapIfNotExists(
          this ICollection<KeycloakClaimMap> list,
          ICollection<KeycloakClaimMap> items)
        {
            foreach (var keycloakClaimMap in items)
            {
                list.AddMapIfNotExists(keycloakClaimMap);
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



            avaibleMaps.AddMapIfNotExists(new KeycloakClaimMap(t => t.UserAuthName, "preferred_username"));
            avaibleMaps.AddMapIfNotExists(new KeycloakClaimMap(t => t.UserAuthId, "sub"));
            avaibleMaps.AddMapIfNotExists(new KeycloakClaimMap(t => t.EmailConfirmed!, "email_verified"));
            avaibleMaps.AddMapIfNotExists(new KeycloakClaimMap(t => t.Roles, "role"));
            avaibleMaps.AddMapIfNotExists(new KeycloakClaimMap(t => t.Permissions, "groups"));
            avaibleMaps.AddMapIfNotExists(new KeycloakClaimMap(t => t.DisplayName, "name"));
            avaibleMaps.AddMapIfNotExists(new KeycloakClaimMap(t => t.UserName, "preferred_username"));
            avaibleMaps.AddMapIfNotExists(new KeycloakClaimMap(t => t.FirstName, "given_name"));
            avaibleMaps.AddMapIfNotExists(new KeycloakClaimMap(t => t.LastName, "family_name"));
            avaibleMaps.AddMapIfNotExists(new KeycloakClaimMap(t => t.Email, "email"));
 

            avaibleMaps = avaibleMaps.GroupBy(t => t.Property).Select(t => t.First()).ToList();

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
                var converter = TypeDescriptor.GetConverter(property.PropertyType);
                var text = obj.Get(jsonKey);
                if (IsNullableType(property.PropertyType))
                {
                    if (IsPropertyEnumerable(Nullable.GetUnderlyingType(property.PropertyType)))
                    {
                        var obj1 = JsonSerializer.DeserializeFromString(obj.GetArray<object>(jsonKey).SerializeToString(), property.PropertyType);
                        property.SetValue(session, obj1, null);
                    }
                    else
                    {
                        var obj2 = converter.ConvertFromString(text);
                        property.SetValue(session, obj2, null);
                    }
                }
                else if (IsPropertyEnumerable(property.PropertyType))
                {
                    var obj3 = JsonSerializer.DeserializeFromString(obj.GetArray<object>(jsonKey).SerializeToString(), property.PropertyType);
                    property.SetValue(session, obj3, null);
                }
                else
                {
                    var obj4 = converter.ConvertFromString(text);
                    property.SetValue(session, obj4, null);
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
