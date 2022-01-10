using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Smallprogram.BlazorServerClient.Data;
using Smallprogram.BlazorServerClient.Service;

var builder = WebApplication.CreateBuilder(args);



#region HttpClient
string IdentityServerHttpUri = builder.Configuration["BaseServerUri:HttpServerUri:IdentityServer"];
string Api1HttpUri = builder.Configuration["BaseServerUri:HttpServerUri:Api1"];

string IdentityServerHttpsUri = builder.Configuration["BaseServerUri:HttpsServerUri:IdentityServer"];
string Api1HttpsUri = builder.Configuration["BaseServerUri:HttpsServerUri:Api1"];

bool is_https = true;

if (is_https)
{
    builder.Services.AddHttpClient("identityServer", op =>
    {
        op.BaseAddress = new Uri(IdentityServerHttpsUri);
    });
    builder.Services.AddHttpClient("api1", op =>
    {
        op.BaseAddress = new Uri(Api1HttpsUri);
    });
}
else
{
    builder.Services.AddHttpClient("IdentityServer", op =>
    {
        op.BaseAddress = new Uri(IdentityServerHttpUri);
    });
    builder.Services.AddHttpClient("api1", op =>
    {
        op.BaseAddress = new Uri(Api1HttpUri);
    });
}

#endregion

#region OIDC
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

})
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(50);
        options.SlidingExpiration = false;
        //options.Cookie.Name = "ZhuSirBlazorServerClient";

    })
    .AddOpenIdConnect(options =>
    {
        options.NonceCookie.SameSite = SameSiteMode.Unspecified;
        options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
        // Note: these settings must match the application details
        // inserted in the database at the server level.
        options.ClientId = "blazor-Server-client";
        options.ClientSecret = "7f71f2f5-2fd6-43bb-9b5a-19c5b52d602d";

        options.RequireHttpsMetadata = false;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;

        // Use the authorization code flow.
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;


        // Note: setting the Authority allows the OIDC client middleware to automatically
        // retrieve the identity provider's configuration and spare you from setting
        // the different endpoints URIs or the token validation parameters explicitly.\
        if (is_https)
        {
            options.Authority = IdentityServerHttpsUri;
        }
        else
        {
            options.Authority = IdentityServerHttpUri;
        }


        options.Scope.Clear();
        options.Scope.Add(OpenIdConnectScope.OpenId);
        options.Scope.Add(OpenIdConnectScope.Email);
        options.Scope.Add(OpenIdConnectScope.OpenIdProfile);
        options.Scope.Add(OpenIdConnectScope.OfflineAccess);
        options.Scope.Add("roles");
        options.Scope.Add("api1");

        // Disable the built-in JWT claims mapping feature.
        options.MapInboundClaims = false;

        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "role";

        //options.AccessDeniedPath = "/";
    });

#endregion

#region Services

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CallApiService>();

#endregion


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapBlazorHub();
app.MapControllers();
app.MapDefaultControllerRoute();
app.MapFallbackToPage("/_Host");

app.Run();
