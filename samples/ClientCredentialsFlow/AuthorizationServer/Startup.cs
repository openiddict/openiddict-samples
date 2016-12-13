﻿using System.Linq;
using AuthorizationServer.Models;
using CryptoHelper;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Core;
using OpenIddict.Models;

namespace AuthorizationServer {
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc();

            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase());

            // Register the OpenIddict services.
            services.AddOpenIddict()
                // Register the Entity Framework stores.
                .AddEntityFrameworkCoreStores<ApplicationDbContext>()

                // Register the ASP.NET Core MVC binder used by OpenIddict.
                // Note: if you don't call this method, you won't be able to
                // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                .AddMvcBinders()

                // Enable the token endpoint.
                .EnableTokenEndpoint("/connect/token")

                // Enable the client credentials flow.
                .AllowClientCredentialsFlow()

                // During development, you can disable the HTTPS requirement.
                .DisableHttpsRequirement()

                // Register a new ephemeral key, that is discarded when the application
                // shuts down. Tokens signed using this key are automatically invalidated.
                // This method should only be used during development.
                .AddEphemeralSigningKey();

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

            using (var context = new ApplicationDbContext(
                app.ApplicationServices.GetRequiredService<DbContextOptions<ApplicationDbContext>>())) {
                context.Database.EnsureCreated();

                var applications = context.Set<OpenIddictApplication>();

                if (!applications.Any()) {
                    applications.Add(new OpenIddictApplication {
                        ClientId = "AB08936C-5049-4499-9958-1B8E6E218743",
                        ClientSecret = Crypto.HashPassword("388D45FA-B36B-4988-BA59-B187D329C207"),
                        DisplayName = "My client application",
                        Type = OpenIddictConstants.ClientTypes.Confidential
                    });

                    context.SaveChanges();
                }
            }
        }
    }
}
