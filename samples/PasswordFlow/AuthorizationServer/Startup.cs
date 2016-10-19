using AuthorizationServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthorizationServer {
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc();

            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase());

            // Register the Identity services.
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Register the OpenIddict services, including the default Entity Framework stores.
            services.AddOpenIddict<ApplicationDbContext>()
                // Register the ASP.NET Core MVC binder used by OpenIddict.
                // Note: if you don't call this method, you won't be able to
                // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                .AddMvcBinders()

                // Enable the token endpoint.
                .EnableTokenEndpoint("/connect/token")

                // Enable the password and the refresh token flows.
                .AllowPasswordFlow()
                .AllowRefreshTokenFlow()

                // During development, you can disable the HTTPS requirement.
                .DisableHttpsRequirement()

                // Register a new ephemeral key, that is discarded when the application
                // shuts down. Tokens signed using this key are automatically invalidated.
                // This method should only be used during development.
                .AddEphemeralSigningKey();

            // Note: if you don't explicitly register a signing key, one is automatically generated and
            // persisted on the disk. If the key cannot be persisted, an exception is thrown.
            // 
            // On production, using a X.509 certificate stored in the machine store is recommended.
            // You can generate a self-signed certificate using Pluralsight's self-cert utility:
            // https://s3.amazonaws.com/pluralsight-free/keith-brown/samples/SelfCert.zip
            // 
            // services.AddOpenIddict<ApplicationDbContext>()
            //     .AddSigningCertificate("7D2A741FE34CC2C7369237A5F2078988E17A6A75");
            // 
            // Alternatively, you can also store the certificate as an embedded .pfx resource
            // directly in this assembly or in a file published alongside this project:
            // 
            // services.AddOpenIddict<ApplicationDbContext>()
            //     .AddSigningCertificate(
            //          assembly: typeof(Startup).GetTypeInfo().Assembly,
            //          resource: "AuthorizationServer.Certificate.pfx",
            //          password: "OpenIddict");
        }

        public void Configure(IApplicationBuilder app) {
            app.UseDeveloperExceptionPage();

            // Add a middleware used to validate access
            // tokens and protect the API endpoints.
            app.UseOAuthValidation();

            // Alternatively, you can also use the introspection middleware.
            // Using it is recommended if your resource server is in a
            // different application/separated from the authorization server.
            // 
            // app.UseOAuthIntrospection(options => {
            //     options.AutomaticAuthenticate = true;
            //     options.AutomaticChallenge = true;
            //     options.Authority = "http://localhost:54540/";
            //     options.Audience = "resource_server";
            //     options.ClientId = "resource_server";
            //     options.ClientSecret = "875sqd4s5d748z78z7ds1ff8zz8814ff88ed8ea4z4zzd";
            // });

            app.UseOpenIddict();

            app.UseMvcWithDefaultRoute();

            app.UseWelcomePage();
        }
    }
}
