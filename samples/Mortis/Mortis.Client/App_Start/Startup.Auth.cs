using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

namespace Mortis.Client
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Authority = "https://localhost:44349/",

                ClientId = "mvc",
                ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",

                RedirectUri = "https://localhost:44378/signin-oidc",

                RedeemCode = true,

                ResponseMode = OpenIdConnectResponseMode.Query,
                ResponseType = OpenIdConnectResponseType.Code,

                Scope = "openid profile email roles",

                SecurityTokenValidator = new JwtSecurityTokenHandler
                {
                    // Disable the built-in JWT claims mapping feature.
                    InboundClaimTypeMap = new Dictionary<string, string>()
                },

                TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                },

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    // Note: by default, the OIDC client throws an OpenIdConnectProtocolException
                    // when an error occurred during the authentication/authorization process.
                    // To prevent a YSOD from being displayed, the response is declared as handled.
                    AuthenticationFailed = notification =>
                    {
                        if (string.Equals(notification.ProtocolMessage.Error, "access_denied", StringComparison.Ordinal))
                        {
                            notification.HandleResponse();

                            notification.Response.Redirect("/");
                        }

                        return Task.CompletedTask;
                    }
                }
            });
        }
    }
}
