using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Balosar.Client
{
    public static class Program
    {
        public static Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient("Balosar.ServerAPI")
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project.
            builder.Services.AddScoped(provider =>
            {
                var factory = provider.GetRequiredService<IHttpClientFactory>();
                return factory.CreateClient("Balosar.ServerAPI");
            });

            builder.Services.AddOidcAuthentication(options =>
            {
                options.ProviderOptions.ClientId = "balosar-blazor-client";
                options.ProviderOptions.Authority = "https://localhost:44310/";
                options.ProviderOptions.ResponseType = "code";
                options.ProviderOptions.ResponseMode = "fragment";
                options.AuthenticationPaths.RemoteRegisterPath = "https://localhost:44310/Identity/Account/Register";
            });

            var host = builder.Build();
            return host.RunAsync();
        }
    }
}
