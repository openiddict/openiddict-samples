# OpenIddict samples

This repository contains samples demonstrating **how to use [OpenIddict](https://github.com/openiddict/openiddict-core) with the different OAuth 2.0/OpenID Connect flows**.

## ASP.NET Core samples

  - [Aridka](samples/Aridka): client credentials demo, with a .NET console acting as the client.
  - [Balosar](samples/Balosar): authorization code flow demo, with a Blazor WASM application acting as the client.
  - [Contruum](samples/Contruum): conformance tests project using Razor Pages and 2 hardcoded user identities, meant to be used with [the OIDC certification suite](https://www.certification.openid.net/).
  - [Dantooine](samples/Dantooine): backend-for-frontend (BFF) Blazor WASM application hosted in ASP.NET Core with Microsoft YARP for downstream API.
  - [Geonosis](sample/Geonosis): Blazor Web with InteractiveAuto mode, backend-for-frontend (BFF) with YARP and token exchange flow to call a downstream API.
  - [Hollastin](samples/Hollastin): resource owner password credentials demo, with a .NET console acting as the client.
  - [Imynusoph](samples/Imynusoph): refresh token grant demo, with a .NET console acting as the client.
  - [Matty](samples/Matty): device authorization flow demo, with a .NET console acting as the client.
  - [Mimban](samples/Mimban): authorization code flow demo using minimal APIs and GitHub delegation for user authentication, with a .NET console acting as the client.
  - [Velusia](samples/Velusia): authorization code flow demo, with an ASP.NET Core application acting as the client.
  - [Weytta](samples/Weytta): authorization code flow with Integrated Windows Authentication support and a .NET console acting as the client.
  - [Zirku](samples/Zirku): authorization code flow demo using minimal APIs with 2 hard-coded user identities, a .NET console and a SPA acting as the clients and two API projects using introspection (Api1) and local validation (Api2).

## .NET samples

  - [Sorgan](samples/Sorgan): console, Windows Forms, Windows Presentation Foundation and Blazor Hybrid clients using GitHub for user authentication.

## OWIN/ASP.NET 4.8 samples
  - [Fornax](samples/Fornax): authorization code flow demo using ASP.NET Web Forms 4.8 and OWIN/Katana, with a .NET Framework 4.8 console acting as the client.
  - [Mortis](samples/Mortis): authorization code flow demo using ASP.NET MVC 5.2 and ASP.NET Web API 2.2, with an ASP.NET MVC 5.2 application acting as the client.
  - [Kalarba](samples/Kalarba): resource owner password credentials demo using OWIN/Katana, ASP.NET Web API 2.2 and the OpenIddict degraded mode.

## External samples

**Looking for additional samples to help you get started with OpenIddict?** Don't miss these interesting samples maintained by the community:

  - **[Angular and Blazor samples](https://github.com/damienbod/AspNetCoreOpeniddict)** by [Damien Bowden](https://github.com/damienbod)
  - **[MAUI client sandbox (iOS, Mac Catalyst and WinUI 3)](https://github.com/openiddict/openiddict-core/tree/dev/sandbox/OpenIddict.Sandbox.Maui.Client)** by [Kévin Chalet](https://github.com/kevinchalet)
  - **[OIDC session management sample](https://github.com/GREsau/openiddict-session-management-sample)** by [Graham Esau](https://github.com/GREsau)

--------------

## Certification

Unlike many other identity providers, **OpenIddict is not a turnkey solution but a framework that requires writing custom code**
to be operational (typically, at least an authorization controller), making it a poor candidate for the certification program.

While a reference implementation could be submitted as-is, **this wouldn't guarantee that implementations deployed by OpenIddict users would be standard-compliant.**

Instead, **developers are encouraged to execute the conformance tests against their own deployment** once they've implemented their own logic.

> [!TIP]
> This repository contains [a dedicated sample](https://github.com/openiddict/openiddict-samples/tree/dev/samples/Contruum/Contruum.Server) specially designed to be used
> with the OpenID Connect Provider Certification tool and demonstrate that OpenIddict can be easily used in a certified implementation. To allow executing the certification tests
> as fast as possible, that sample doesn't include any membership or consent feature (two hardcoded identities are proposed for tests that require switching between identities).

--------------

## Security policy

Security issues and bugs should be reported privately by emailing [security@openiddict.com](mailto:security@openiddict.com).
You should receive a response within 24 hours. If for some reason you do not, please follow up via email to ensure we received your original message.

--------------

## Support policy

**If you need support, please first make sure you're [sponsoring the project](https://github.com/sponsors/kevinchalet) or are a regular contributor**:
GitHub tickets opened by users who haven't contributed to the project or don't actively sponsor it will be automatically closed.

> [!TIP]
> Depending on the tier you selected, you can open a GitHub ticket or send an email to [contact@openiddict.com](mailto:contact@openiddict.com) for private support.
>
> Alternatively, you can also post your question on [Gitter](https://app.gitter.im/#/room/#openiddict_openiddict-core:gitter.im).

**Support is only offered for the latest stable version of OpenIddict**. There are, however, two exceptions to this policy:
  - **ABP Framework users receive patches for OpenIddict for as long as ABP Framework itself is supported by Volosoft**
  (typically a year following the release of a major ABP version), whether they have a commercial ABP license or just use the free packages:

  | OpenIddict branch | ABP Framework branch | End of support date (estimated) |
  |-------------------|----------------------|---------------------------------|
  | 4.x               | 7.x                  | December 19, 2024               |
  | 5.x               | 8.x                  | November 19, 2025               |
  | 6.x               | 9.x                  | Currently supported             |
  | 7.x (current)     | Not supported yet    | Not supported yet               |

  - **OpenIddict sponsors are offered extended support depending on the selected sponsorship tier:**
    - Tier 6 sponsors get full support for the previous version 1 month following the release of a new major version.
    - Tier 7 sponsors get full support for the previous version 6 months following the release of a new major version.
    - Tier 8 sponsors get full support for the previous version 12 months following the release of a new major version.
    - Tier 9 sponsors get full support for the previous version 24 months following the release of a new major version.

  | OpenIddict branch | Sponsorship tier | End of support date |
  |-------------------|------------------|---------------------|
  | 4.x               | Tier 6           | January 18, 2024    |
  | 4.x               | Tier 7           | June 18, 2024       |
  | 4.x               | Tier 8           | December 18, 2024   |
  | 4.x               | Tier 9           | December 18, 2025   |
  |                   |                  |                     |
  | 5.x               | Tier 6           | January 17, 2025    |
  | 5.x               | Tier 7           | June 17, 2025       |
  | 5.x               | Tier 8           | December 17, 2025   |
  | 5.x               | Tier 9           | December 17, 2026   |
  |                   |                  |                     |
  | 6.x               | Tier 6           | August 7, 2025      |
  | 6.x               | Tier 7           | January 7, 2026     |
  | 6.x               | Tier 8           | July 7, 2026        |
  | 6.x               | Tier 9           | July 7, 2027        |
  |                   |                  |                     |
  | 7.x (current)     | Any              | Currently supported |

> [!TIP]
> For more information on the different tiers and the benefits they offer, visit [GitHub Sponsors](https://github.com/sponsors/kevinchalet).

--------------

## Contributors

**OpenIddict** is actively maintained by **[Kévin Chalet](https://github.com/kevinchalet)**. Contributions are welcome and can be submitted using pull requests.

--------------

## License

This project is licensed under the **Apache License**. This means that you can use, modify and distribute it freely. See [http://www.apache.org/licenses/LICENSE-2.0.html](http://www.apache.org/licenses/LICENSE-2.0.html) for more details.
