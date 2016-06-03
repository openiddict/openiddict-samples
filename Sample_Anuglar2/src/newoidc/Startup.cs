using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict;
using OpenIddict.Models;
using newoidc.Data;
using newoidc.Models;
using newoidc.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NWebsec.AspNetCore.Middleware;

namespace newoidc
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
       
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
      /*
            services.AddAuthentication(options => {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });*/
            // Add framework services.
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(o => {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddOpenIddict();

            services.AddMvc();

          

    
            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
           // new TextWriterTraceListener(writer: Console.Out));
            // app.UseDeveloperExceptionPage();
            // app.UseDatabaseErrorPage();
            // app.UseBrowserLink();

            app.UseStaticFiles();
          // app.UseOAuthValidation();
           //app.UseIdentity();
           
/*
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                RequireHttpsMetadata = false,
                Audience = "http://localhost:58056/",
                Authority = "http://localhost:58056/"
            });*/
            // This must be *after* "app.UseIdentity();" above
          
         
           
            app.UseIdentity();
            app.UseGoogleAuthentication(new GoogleOptions
            {
                ClientId = "862227465575-q1spfclcfvflg4tpesfkle4e0jc3q987.apps.googleusercontent.com",
                ClientSecret = "ozzg2VvH2TYbSbBYWE_HIYG5"

            });

            app.UseTwitterAuthentication(new TwitterOptions
            {
                ConsumerKey = "6XaCTaLbMqfj6ww3zvZ5g",
                ConsumerSecret = "Il2eFzGIrYhz6BWjYhVXBPQSfZuS4xoHpSSyD9PI"
            });
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                RequireHttpsMetadata = false,
                Audience = "http://localhost:58056/",
                Authority = "http://localhost:58056/"
            });
            app.UseOpenIddict(options =>
            {
                 options.Options.UseJwtTokens();
                options.Options.AllowInsecureHttp = true;
                options.UseNWebsec(directives =>
                {
                    directives.DefaultSources(directive => directive.Self().CustomSources("*"))
                        .ImageSources(directive => directive.Self().CustomSources("*", "data:"))
                        .ScriptSources(directive => directive
                            .Self()
                            .UnsafeEval()
                            .UnsafeInline()
                            .CustomSources("*"))
                        .StyleSources(directive => directive
                        .Self()
                         .CustomSources("*")
                            .UnsafeInline());
                });
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "spa-fallback",
                    template: "{*url}",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
