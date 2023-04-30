using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Dantooine.WebAssembly.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dantooine.WebAssembly.Client;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.Services.AddOptions();
        builder.Services.AddAuthorizationCore();
        builder.Services.TryAddSingleton<AuthenticationStateProvider, HostAuthenticationStateProvider>();
        builder.Services.TryAddSingleton(provider => (HostAuthenticationStateProvider) provider.GetRequiredService<AuthenticationStateProvider>());
        builder.Services.AddTransient<AuthorizedHandler>();

        builder.RootComponents.Add<App>("#app");

        builder.Services.AddHttpClient("default", client =>
        {
            client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        builder.Services.AddHttpClient("authorizedClient", client =>
        {
            client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddHttpMessageHandler<AuthorizedHandler>();

        builder.Services.AddTransient(provider => provider.GetRequiredService<IHttpClientFactory>().CreateClient("default"));

        var host = builder.Build();
        await host.RunAsync();
    }
}
