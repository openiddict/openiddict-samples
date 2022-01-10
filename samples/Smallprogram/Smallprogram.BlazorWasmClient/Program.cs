using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Smallprogram.BlazorWasmClient;
using Smallprogram.BlazorWasmClient.Service;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

string BaseHttpUri = builder.HostEnvironment.BaseAddress;


string IdentityServerHttpUri = builder.Configuration["BaseServerUri:HttpServerUri:IdentityServer"];
string Api1HttpUri = builder.Configuration["BaseServerUri:HttpServerUri:Api1"];

string IdentityServerHttpsUri = builder.Configuration["BaseServerUri:HttpsServerUri:IdentityServer"];
string Api1HttpsUri = builder.Configuration["BaseServerUri:HttpsServerUri:Api1"];

bool is_https = true;

if (is_https)
{
    builder.Services.AddHttpClient("Smallprogram.Server")
    .ConfigureHttpClient(client => client.BaseAddress = new Uri(IdentityServerHttpsUri))
    .AddHttpMessageHandler(sp =>
    {
        var handler = sp.GetService<AuthorizationMessageHandler>()
            .ConfigureHandler(
                authorizedUrls: new[] { IdentityServerHttpsUri },
                scopes: new[] { "openid", "email", "roles", "profile","api1" }
            );
        return handler;
    });

     builder.Services.AddHttpClient("Smallprogram.Api1")
    .ConfigureHttpClient(client => client.BaseAddress = new Uri(Api1HttpsUri))
    .AddHttpMessageHandler(sp =>
    {
        var handler = sp.GetService<AuthorizationMessageHandler>()
            .ConfigureHandler(
                authorizedUrls: new[] { Api1HttpsUri },
                scopes: new[] { "api1" }
            );
        return handler;
    });

}
else
{
    builder.Services.AddHttpClient("Smallprogram.Server")
    .ConfigureHttpClient(client => client.BaseAddress = new Uri(IdentityServerHttpUri))
    .AddHttpMessageHandler(sp =>
    {
        var handler = sp.GetService<AuthorizationMessageHandler>()
            .ConfigureHandler(
                authorizedUrls: new[] { IdentityServerHttpUri },
                scopes: new[] { "openid", "email", "roles", "profile", "api1" }
            );
        return handler;
    });

   
    builder.Services.AddHttpClient("Smallprogram.Api1")
    .ConfigureHttpClient(client => client.BaseAddress = new Uri(Api1HttpUri))
    .AddHttpMessageHandler(sp =>
    {
        var handler = sp.GetService<AuthorizationMessageHandler>()
            .ConfigureHandler(
                authorizedUrls: new[] { Api1HttpUri },
                scopes: new[] { "api1" }
            );
        return handler;
    });
}


builder.Services.AddOidcAuthentication(options =>
{
    // Configure your authentication provider options here.
    // For more information, see https://aka.ms/blazor-standalone-auth
    //builder.Configuration.Bind("Local", options.ProviderOptions);

    options.ProviderOptions.ClientId = "blazor-wasm-client";
    if (is_https)
    {
        options.ProviderOptions.Authority = IdentityServerHttpsUri; //IdentityServerUri
        options.AuthenticationPaths.RemoteRegisterPath = IdentityServerHttpsUri + "Identity/Account/Register";
    }
    else
    {
        options.ProviderOptions.Authority = IdentityServerHttpUri;
        options.AuthenticationPaths.RemoteRegisterPath = IdentityServerHttpUri + "Identity/Account/Register";
    }

    options.UserOptions.NameClaim = "name";
    options.UserOptions.RoleClaim = "role";

    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Clear();
    options.ProviderOptions.DefaultScopes.Add("openid email roles profile");
    options.ProviderOptions.DefaultScopes.Add("api1");

    // Note: response_mode=fragment is the best option for a SPA. Unfortunately, the Blazor WASM
    // authentication stack is impacted by a bug that prevents it from correctly extracting
    // authorization error responses (e.g error=access_denied responses) from the URL fragment.
    // For more information about this bug, visit https://github.com/dotnet/aspnetcore/issues/28344.
    //
    options.ProviderOptions.ResponseMode = "query";


});


builder.Services.AddScoped<CallApiService>();



await builder.Build().RunAsync();
