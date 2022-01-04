using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Net.Http;
using Yarp.ReverseProxy.Forwarder;

namespace Blazor.BFF.OpenIddict.Server
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
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            });

            services.AddHttpClient();
            services.AddOptions();
            ;
            var openIDConnectSettings = Configuration.GetSection("OpenIDConnectSettings");

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
           .AddCookie()
           .AddOpenIdConnect(options =>
           {
               options.SignInScheme = "Cookies";
               options.Authority = openIDConnectSettings["Authority"];
               options.ClientId = openIDConnectSettings["ClientId"];
               options.ClientSecret = openIDConnectSettings["ClientSecret"];
               options.RequireHttpsMetadata = true;
               options.ResponseType = "code";
               options.UsePkce = true;
               options.Scope.Add("profile");
               options.Scope.Add("offline_access");
               options.Scope.Add("dataEventRecords");
               options.SaveTokens = true;
               options.GetClaimsFromUserInfoEndpoint = true;
               //options.ClaimActions.MapUniqueJsonKey("preferred_username", "preferred_username");
           });

            services.AddControllersWithViews(options =>
                 options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

            services.AddRazorPages().AddMvcOptions(options =>
            {
                //var policy = new AuthorizationPolicyBuilder()
                //    .RequireAuthenticatedUser()
                //    .Build();
                //options.Filters.Add(new AuthorizeFilter(policy));
            });


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
                    Configuration["OpenIDConnectSettings:Authority"]));

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
                endpoints.Map("/api/DataEventRecords", async httpContext =>
                {
                    var error = await forwarder.SendAsync(httpContext, "https://localhost:44390/",
                        httpClient, requestConfig, transformer);
                    // Check if the operation was successful
                    if (error != ForwarderError.None)
                    {
                        var errorFeature = httpContext.GetForwarderErrorFeature();
                        var exception = errorFeature.Exception;
                    }
                }).RequireAuthorization();

                endpoints.MapFallbackToPage("/_Host");

            });
        }
    }
}
