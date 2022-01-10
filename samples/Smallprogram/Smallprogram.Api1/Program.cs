using OpenIddict.Validation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


#region OpenIddict Validation

//bool is_https = true;
//var IssuerUri = "";
//if (is_https)
//{
//    IssuerUri = builder.Configuration.GetSection("httpsIssuer").Value;

//}
//else
//{
//    IssuerUri = builder.Configuration.GetSection("httpIssuer").Value;
//}

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;

});

// Register the OpenIddict validation components.
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {

        // Note: the validation handler uses OpenID Connect discovery
        // to retrieve the address of the introspection endpoint.
        //options.SetIssuer(IssuerUri);
        options.SetIssuer("https://localhost:9000/");
        options.AddAudiences("resource_server_1");

        // Configure the validation handler to use introspection and register the client
        // credentials used when communicating with the remote introspection endpoint.
        options.UseIntrospection()
           .SetClientId("resource_server_1")
           .SetClientSecret("1de4db00-e45b-4200-8d9a-0a6317d8e307");

        // Register the System.Net.Http integration.
        options.UseSystemNetHttp();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });
#endregion

builder.Services.AddCors(options =>
{
    options.AddPolicy("all", builder =>
    {
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();
        builder.AllowAnyOrigin();
    });
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("all");


app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
