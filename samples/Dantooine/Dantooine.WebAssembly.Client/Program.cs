using Dantooine.WebAssembly.Client;
using Dantooine.WebAssembly.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();

builder.Services.AddHttpClient(Options.DefaultName)
    .ConfigureHttpClient(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

builder.Services.TryAddSingleton<AuthenticationStateProvider, HostAuthenticationStateProvider>();

builder.RootComponents.Add<App>("#app");

var host = builder.Build();
await host.RunAsync();