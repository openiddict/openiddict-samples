using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Yarp.ReverseProxy.Forwarder;

namespace Dantooine.BFF.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
                options.Cookie.Name = "__Host-X-XSRF-TOKEN";
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.AddHttpClient();
            services.AddOptions();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
           .AddCookie()
           .AddOpenIdConnect(options =>
           {
               options.SignInScheme = "Cookies";
               options.Authority = "https://localhost:44319";
               options.ClientId = "blazorcodeflowpkceclient";
               options.ClientSecret = "codeflow_pkce_client_secret";
               options.RequireHttpsMetadata = true;
               options.ResponseType = OpenIdConnectResponseType.Code;
               options.Scope.Add("profile");
               options.Scope.Add("api1");
               options.SaveTokens = true;
               options.GetClaimsFromUserInfoEndpoint = true;
           });

            services.AddControllersWithViews(options =>
                 options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

            services.AddRazorPages();
            services.AddHttpForwarder();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpForwarder forwarder)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseSecurityHeaders(
                SecurityHeadersDefinitions.GetHeaderPolicyCollection(env.IsDevelopment(),
                    "https://localhost:44319"));

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                UseCookies = false
            });
            var transformer = new CookieTokenTransformer(); // or HttpTransformer.Default;
            var requestConfig = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.Map("/api/DantooineApi1", async httpContext =>
                {
                    await forwarder.SendAsync(httpContext, "https://localhost:44343/", httpClient, requestConfig, transformer);
                }).RequireAuthorization();

                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
