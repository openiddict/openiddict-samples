using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Balosar.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddHttpClient("Balosar.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Balosar.ServerAPI"));

            builder.Services.AddOidcAuthentication(cfg =>
            {
                cfg.ProviderOptions.MetadataUrl = "https://localhost:44310/.well-known/openid-configuration";
                cfg.ProviderOptions.ClientId = "balosar-blazor-client";
                cfg.ProviderOptions.Authority = "https://localhost:44310";
                cfg.ProviderOptions.ResponseType = "code";
                cfg.ProviderOptions.ResponseMode = "fragment";
                cfg.AuthenticationPaths.RemoteRegisterPath = "https://localhost:44310/Identity/Account/Register";
            });

            await builder.Build().RunAsync();
        }
    }
}
