
using System.Linq.Expressions;
using Be.Auto.Servicestack.Authentication.Keycloak.Claim;
using Be.Auto.Servicestack.Authentication.Keycloak.Extensions;
using Be.Auto.Servicestack.Authentication.Keycloak.Helpers;
using Be.Auto.Servicestack.Authentication.Keycloak.Providers.Model;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Configuration;
using ServiceStack.Host;
using ServiceStack.Text;
using ServiceStack.Web;

namespace Be.Auto.Servicestack.Authentication.Keycloak.Providers
{
    public class KeycloakOAuthProvider : OAuthProvider, IKeycloakAuthProvider
    {
        public const string Name = KeycloakAuthProviders.OAuthProvider;
        private readonly List<KeycloakClaimMap> _maps = new List<KeycloakClaimMap>();
        public string Scope { get; set; }
        public KeycloakOAuthProvider(IAppSettings appSettings)
      : base(appSettings, "Keycloak:AuthRealm", Name, "ClientId", "ClientSecret")
        {
            AuthRealm = appSettings.GetString("Keycloak:AuthRealm");
            ConsumerKey = appSettings.GetString("Keycloak:ClientId");
            ConsumerSecret = appSettings.GetString("Keycloak:ClientSecret");
            Scope = appSettings.GetString("Keycloak:Scope") ?? "openid profile email address phone";
            NavItem = new NavItem
            {
                Href = $"/auth/{Name}",
                Label = "Sign In with Keycloak",
                Id = $"btn-{Name}",
                ClassName = $"btn-social {Name}"
            };
            Icon = Svg.ImageSvg("<svg width='800px' height='800px' viewBox='0 0 1024 1024' xmlns='http://www.w3.org/2000/svg'>\r\n                                  <circle cx='512' cy='512' r='512' style='fill:#008aaa'/>\r\n                                  <path d='M786.2 395.5h-80.6c-1.5 0-3-.8-3.7-2.1l-64.7-112.2c-.8-1.3-2.2-2.1-3.8-2.1h-264c-1.5 0-3 .8-3.7 2.1l-67.3 116.4-64.8 112.2c-.7 1.3-.7 2.9 0 4.3l64.8 112.2 67.2 116.5c.7 1.3 2.2 2.2 3.7 2.1h264.1c1.5 0 3-.8 3.8-2.1L702 630.6c.7-1.3 2.2-2.2 3.7-2.1h80.6c2.7 0 4.8-2.2 4.8-4.8V400.4c-.1-2.7-2.3-4.9-4.9-4.9zM477.5 630.6l-20.3 35c-.3.5-.8 1-1.3 1.3-.6.3-1.2.5-1.9.5h-40.3c-1.4 0-2.7-.7-3.3-2l-60.1-104.3-5.9-10.3-21.6-36.9c-.3-.5-.5-1.1-.4-1.8 0-.6.2-1.3.5-1.8l21.7-37.6 65.9-114c.7-1.2 2-2 3.3-2H454c.7 0 1.4.2 2.1.5.5.3 1 .7 1.3 1.3l20.3 35.2c.6 1.2.5 2.7-.2 3.8l-65.1 112.8c-.3.5-.4 1.1-.4 1.6 0 .6.2 1.1.4 1.6l65.1 112.7c.9 1.5.8 3.1 0 4.4zm202.1-116.7L658 550.8l-5.9 10.3L592 665.4c-.7 1.2-1.9 2-3.3 2h-40.3c-.7 0-1.3-.2-1.9-.5-.5-.3-1-.7-1.3-1.3l-20.3-35c-.9-1.3-.9-2.9-.1-4.2l65.1-112.7c.3-.5.4-1.1.4-1.6 0-.6-.2-1.1-.4-1.6l-65.1-112.8c-.7-1.2-.8-2.6-.2-3.8l20.3-35.2c.3-.5.8-1 1.3-1.3.6-.4 1.3-.5 2.1-.5h40.4c1.4 0 2.7.7 3.3 2l65.9 114 21.7 37.6c.3.6.5 1.2.5 1.8 0 .4-.2 1-.5 1.6z' style='fill:#fff'/>\r\n                                 </svg>");

            InitEndPoints();
        }

        private void InitEndPoints()
        {
            var endPoints = KeycloakEndPointHelper.FindEndPoints(AuthRealm);
            AccessTokenUrl = endPoints.TokenUrl;
            AuthorizeUrl = endPoints.AuthorizeUrl;
            RequestTokenUrl = endPoints.TokenUrl;
            VerifyTokenUrl = endPoints.IntrospectUrl;
            UserProfileUrl = endPoints.UserInfoUrl;
        }

        public override bool IsAuthorized(IAuthSession session, IAuthTokens tokens, Authenticate request = null)
        {
            if (session.IsAuthenticated)
            {
                return base.IsAuthorized(session, tokens, request);
            }

            var currentRequest = HostContext.TryResolve<IHttpContextAccessor>()?.HttpContext?.GetOrCreateRequest();

            if (currentRequest == null)
            {
                return base.IsAuthorized(session, tokens, request);
            }

            var authorization = currentRequest.GetAuthorization();

            if (authorization == null)
            {
                return base.IsAuthorized(session, tokens, request);
            }

            tokens = new AuthTokens();

            if (authorization.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                var bearerToken = currentRequest.GetBearerToken();

                if (bearerToken.IsNullOrEmpty())
                {
                    return base.IsAuthorized(session, tokens, request);
                }

                var accessTokenIsValid = VerifyTokenUrl.CheckAccessTokenIsValid(ConsumerKey, ConsumerKey, bearerToken);

                if (!accessTokenIsValid)
                {
                    return base.IsAuthorized(session, tokens, request);
                }

                session.IsAuthenticated = accessTokenIsValid;
                tokens.AccessToken = bearerToken;
                tokens.AccessTokenSecret = bearerToken;
                _maps.MapClaims(UserProfileUrl, bearerToken, session, tokens).Wait();
                currentRequest.SaveSessionAsync(session).Wait();
                return base.IsAuthorized(session, tokens, request);
            }

            if (authorization.StartsWith("Basic", StringComparison.OrdinalIgnoreCase))
            {
                var credentials = currentRequest.GetBasicAuthUserAndPassword();

                if (credentials == null)
                {
                    return base.IsAuthorized(session, tokens, request);
                }

                var accessTokenResult = AccessTokenUrl.RequestAccessToken(new
                {
                    grant_type = "password",
                    client_id = ConsumerKey,
                    client_secret = ConsumerSecret,
                    username = credentials.Value.Key,
                    password = credentials.Value.Value,
                    scope = Scope
                });

                MapTokens(session, tokens, accessTokenResult);
                _maps.MapClaims(UserProfileUrl, accessTokenResult.AccessToken, session, tokens).Wait();
                currentRequest.SaveSessionAsync(session).Wait();
                return base.IsAuthorized(session, tokens, request);
            }

            return base.IsAuthorized(session, tokens, request);
        }

        public override async Task<object> AuthenticateAsync(
            IServiceBase authService,
            IAuthSession session,
            Authenticate request,
            CancellationToken token = default(CancellationToken))
        {
            var tokens = Init(authService, ref session, request);
            var ctx = CreateAuthContext(authService, session, tokens);
            var httpRequest = authService.Request;
            var authInfo = new Dictionary<string, string>();
            if (!request.AccessToken.IsNullOrEmpty())
            {
                var accessTokenIsValid = await VerifyTokenUrl.CheckAccessTokenIsValidAsync(ConsumerKey, ConsumerKey, request, token);
                session.IsAuthenticated = accessTokenIsValid;
                tokens.AccessToken = request.AccessToken;
                tokens.AccessTokenSecret = request.AccessToken;
                return await OnAuthenticatedAsync(authService, session, tokens, authInfo, token);

            }
            if (!request.UserName.IsNullOrEmpty() && !request.Password.IsNullOrEmpty())
            {
                var accessTokenResult = await AccessTokenUrl.RequestAccessTokenAsync(new
                {
                    grant_type = "password",
                    client_id = ConsumerKey,
                    client_secret = ConsumerSecret,
                    username = request.UserName,
                    password = request.Password,
                    scope = Scope
                }, cancelToken: token);
                MapTokens(session, tokens, accessTokenResult);
                authInfo = accessTokenResult.ToDictionary();
                return await OnAuthenticatedAsync(authService, session, tokens, authInfo, token);

            }
            var code = httpRequest.QueryString["code"];
            var isPreAuthCallback = !code.IsNullOrEmpty();
            if (!isPreAuthCallback)
            {
                var preAuthUrl = $"{AuthorizeUrl}?response_type=code&client_id={ConsumerKey}&redirect_uri={CallbackUrl.UrlEncode()}&scope={Scope}";
                return authService.Redirect(PreAuthUrlFilter(ctx, preAuthUrl));

            }
            var tokenResult = await AccessTokenUrl.RequestAccessTokenAsync(new
            {
                client_id = ConsumerKey,
                client_secret = ConsumerSecret,
                redirect_uri = CallbackUrl,
                code,
                grant_type = "authorization_code",
                scope = Scope
            }, cancelToken: token);
            MapTokens(session, tokens, tokenResult);
            authInfo = tokenResult.ToDictionary();
            return await OnAuthenticatedAsync(authService, session, tokens, authInfo, token).ConfigAwait<IHttpResult>() ??
                                await authService.Redirect(SuccessRedirectUrlFilter(ctx, session.ReferrerUrl.SetParam("success", "login"))).SuccessAuthResultAsync(authService, session).ConfigAwait<IHttpResult>();

        }

        private static void MapTokens(
          IAuthSession session,
          IAuthTokens tokens,
          KeycloakTokenResponse accessTokenResult)
        {
            tokens.AccessTokenSecret = accessTokenResult.AccessToken;
            tokens.AccessToken = accessTokenResult.AccessToken;
            tokens.RefreshToken = accessTokenResult.RefreshToken;
            tokens.RefreshTokenExpiry = DateTime.Now.AddSeconds(accessTokenResult.RefreshExpiresIn);
            tokens.State = accessTokenResult.SessionState;
            session.IsAuthenticated = !accessTokenResult.AccessToken.IsNullOrEmpty();
        }


        protected override async Task LoadUserAuthInfoAsync(
          AuthUserSession userSession,
          IAuthTokens tokens,
          Dictionary<string, string> authInfo,
          CancellationToken token = default(CancellationToken))
        {
            await _maps.MapClaims(UserProfileUrl, tokens.AccessToken, userSession, tokens);
        }

        public KeycloakOAuthProvider MapClaim(KeycloakClaimMap map)
        {
            _maps.AddMapIfNotExists(map);
            return this;
        }

        public KeycloakOAuthProvider MapClaims(params KeycloakClaimMap[] maps)
        {
            _maps.AddMapIfNotExists(maps);
            return this;
        }

        public KeycloakOAuthProvider MapClaim(
          Expression<Func<AuthUserSession, object>> property,
          string jsonKey)
        {
            MapClaim(new KeycloakClaimMap(property, jsonKey));
            return this;
        }
    }
}
