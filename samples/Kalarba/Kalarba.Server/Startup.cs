using System;
using System.Reflection;
using System.Security.Claims;
using System.Web.Http;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Integration.WebApi;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Server.Owin;
using OpenIddict.Validation.Owin;
using Owin;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Kalarba.Server;

public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        var container = CreateContainer();

        app.UseErrorPage();

        // Register the Autofac scope injector middleware.
        app.UseAutofacLifetimeScopeInjector(container);

        // Register the two OpenIddict server/validation middleware.
        app.UseMiddlewareFromContainer<OpenIddictServerOwinMiddleware>();
        app.UseMiddlewareFromContainer<OpenIddictValidationOwinMiddleware>();

        var configuration = new HttpConfiguration
        {
            DependencyResolver = new AutofacWebApiDependencyResolver(container)
        };

        configuration.MapHttpAttributeRoutes();

        // Configure ASP.NET Web API to use token authentication.
        configuration.Filters.Add(new HostAuthenticationFilter(OpenIddictValidationOwinDefaults.AuthenticationType));

        // Register the Web API/Autofac integration middleware.
        app.UseAutofacWebApi(configuration);
        app.UseWebApi(configuration);
    }

    private static IContainer CreateContainer()
    {
        var services = new ServiceCollection();

        services.AddOpenIddict()

            // Register the OpenIddict server components.
            .AddServer(options =>
            {
                // Enable the token endpoint.
                options.SetTokenEndpointUris("/connect/token");

                // Enable the password and the refresh token flows.
                options.AllowPasswordFlow();

                // Enable the degraded to allow using the server feature without a backing database.
                options.EnableDegradedMode();

                // Accept anonymous clients (i.e clients that don't send a client_id).
                options.AcceptAnonymousClients();

                // Register the signing and encryption credentials.
                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();

                // Register the OWIN host and configure the OWIN-specific options.
                options.UseOwin()
                       .DisableTransportSecurityRequirement();

                // Register an event handler responsible for validating token requests.
                options.AddEventHandler<ValidateTokenRequestContext>(builder =>
                    builder.UseInlineHandler(context =>
                    {
                        // Client authentication is not used in this sample,
                        // so there's nothing specific to validate here.

                        return default;
                    }));

                // Register an event handler responsible for handling token requests.
                options.AddEventHandler<HandleTokenRequestContext>(builder =>
                    builder.UseInlineHandler(context =>
                    {
                        if (!context.Request.IsPasswordGrantType())
                        {
                            throw new InvalidOperationException("The specified grant type is not supported.");
                        }

                        // Validate the username/password parameters.
                        //
                        // In a real world application, you'd use likely use a key derivation function like PBKDF2 to slow
                        // the client secret validation process down and a time-constant comparer to prevent timing attacks.
                        if (!string.Equals(context.Request.Username, "alice@wonderland.com", StringComparison.Ordinal) ||
                            !string.Equals(context.Request.Password, "P@ssw0rd", StringComparison.Ordinal))
                        {
                            context.Reject(
                                error: Errors.InvalidGrant,
                                description: "The username/password couple is invalid.");

                            return default;
                        }

                        // Create a new identity containing the claims used to generate new tokens.
                        var identity = new ClaimsIdentity(OpenIddictServerOwinDefaults.AuthenticationType);
                        identity.AddClaim(new Claim(Claims.Subject, "999d4ea0-164f-4c1b-8585-b83f313995c9"));
                        identity.AddClaim(new Claim(Claims.Name, "Alice").SetDestinations(Destinations.AccessToken));

                        context.SignIn(new ClaimsPrincipal(identity));

                        return default;
                    }));
            })

            // Register the OpenIddict validation components.
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the OWIN host.
                options.UseOwin();
            });

        // Create a new Autofac container and import the OpenIddict services.
        var builder = new ContainerBuilder();
        builder.Populate(services);
        builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

        return builder.Build();
    }
}
