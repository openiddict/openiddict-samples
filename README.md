# OpenIddict samples

This repository contains samples demonstrating **how to use [OpenIddict](https://github.com/openiddict/openiddict-core) with the different OAuth 2.0/OpenID Connect flows**.

## ASP.NET Core samples

  - [Aridka](samples/Aridka): client credentials demo, with a .NET console acting as the client.
  - [Balosar](samples/Balosar): authorization code flow demo, with a Blazor WASM application acting as the client.
  - [Contruum](samples/Contruum): conformance tests project using Razor Pages and 2 hardcoded user identities, meant to be used with [the OIDC certification suite](https://www.certification.openid.net/).
  - [Dantooine](samples/Dantooine): BFF Blazor WASM application hosted in ASP.NET Core with YARP proxy for downstream API.
  - [Hollastin](samples/Hollastin): resource owner password credentials demo, with a .NET console acting as the client.
  - [Imynusoph](samples/Imynusoph): refresh token grant demo, with a .NET console acting as the client.
  - [Matty](samples/Matty): device authorization flow demo, with a .NET console acting as the client.
  - [Mimban](samples/Mimban): authorization code flow demo using minimal APIs and GitHub delegation for user authentication, with a .NET console acting as the client.
  - [Velusia](samples/Velusia): authorization code flow demo, with an ASP.NET Core application acting as the client.
  - [Weytta](samples/Weytta): authorization code flow with Integrated Windows Authentication support and a .NET console acting as the client.
  - [Zirku](samples/Zirku): authorization code flow demo using minimal APIs with 2 hard-coded user identities, a .NET console and a SPA acting as the clients and two API projects using introspection (Api1) and local validation (Api2).

## OWIN/ASP.NET 4.8 samples
  - [Fornax](samples/Fornax): authorization code flow demo using ASP.NET Web Forms 4.8 and OWIN/Katana, with a .NET console acting as the client.
  - [Mortis](samples/Mortis): authorization code flow demo, with an ASP.NET MVC 5.2 application acting as the client.
  - [Kalarba](samples/Kalarba): resource owner password credentials demo using OWIN/Katana, ASP.NET Web API and the OpenIddict degraded mode.

## External samples

**Looking for additional samples to help you get started with OpenIddict?** Don't miss these interesting samples maintained by the community:

  - **[Angular and Blazor samples](https://github.com/damienbod/AspNetCoreOpeniddict)** by [Damien Bowden](https://github.com/damienbod)

## Certification

Unlike many other identity providers, **OpenIddict is not a turnkey solution but a framework that requires writing custom code**
to be operational (typically, at least an authorization controller), making it a poor candidate for the certification program.

While a reference implementation could be submitted as-is, **this wouldn't guarantee that implementations deployed by OpenIddict users would be standard-compliant.**

Instead, **developers are encouraged to execute the conformance tests against their own deployment** once they've implemented their own logic.

> This repository contains [a dedicated sample](https://github.com/openiddict/openiddict-samples/tree/dev/samples/Contruum/Contruum.Server) specially designed to be used
> with the OpenID Connect Provider Certification tool and demonstrate that OpenIddict can be easily used in a certified implementation. To allow executing the certification tests
> as fast as possible, that sample doesn't include any membership or consent feature (two hardcoded identities are proposed for tests that require switching between identities).

## Security policy

Security issues and bugs should be reported privately by emailing security@openiddict.com.
You should receive a response within 24 hours. If for some reason you do not, please follow up via email to ensure we received your original message.

## Support

If you need support, please make sure you [sponsor the project](https://github.com/sponsors/kevinchalet) before creating a GitHub ticket.
If you're not a sponsor, you can post your questions on Gitter or StackOverflow:

- **Gitter: [https://gitter.im/openiddict/openiddict-core](https://gitter.im/openiddict/openiddict-core)**
- **StackOverflow: [https://stackoverflow.com/questions/tagged/openiddict](https://stackoverflow.com/questions/tagged/openiddict)**

## Contributors

**OpenIddict** is actively maintained by **[Kévin Chalet](https://github.com/kevinchalet)**. Contributions are welcome and can be submitted using pull requests.

## License

This project is licensed under the **Apache License**. This means that you can use, modify and distribute it freely. See [http://www.apache.org/licenses/LICENSE-2.0.html](http://www.apache.org/licenses/LICENSE-2.0.html) for more details.
