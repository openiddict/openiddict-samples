# Built using Blazor.BFF.OpenIDConnect.Template

This template can be used to create a Blazor WASM application hosted in an ASP.NET Core Web app using OpenID Connect to authenticate using the BFF security architecture. (server authentication) This removes the tokens form the browser and uses cookies with each HTTP request, response. The template also adds the required security headers as best it can for a Blazor application.

## Features

- WASM hosted in ASP.NET Core 6
- BFF with Azure AD using Microsoft.Identity.Web
- OAuth2 and OpenID Connect OIDC
- No tokens in the browser

## Using the template

### install

```
dotnet new -i Blazor.BFF.OpenIDConnect.Template
```

### run

```
dotnet new blazorbffoidc -n YourCompany.Bff
```

Use the `-n` or `--name` parameter to change the name of the output created. This string is also used to substitute the namespace name in the .cs file for the project.

## Setup after installation

Add the OpenID Connect App registration settings

```
{
  "OpenIDConnectSettings": {
    "Authority": "--your-authority--",
    "ClientId": "--client ID--",
    "ClientSecret": "--client-secret (user secrets)--"
  },
```


### uninstall

```
dotnet new -u Blazor.BFF.OpenIDConnect.Template
```


## Credits, Used NuGet packages + ASP.NET Core 6.0 standard packages

- NetEscapades.AspNetCore.SecurityHeaders
- IdentityModel