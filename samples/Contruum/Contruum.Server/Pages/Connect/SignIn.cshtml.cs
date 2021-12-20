using System;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Contruum.Server.Pages.Connect;

public class SignInModel : PageModel
{
    [FromForm]
    public string? Username { get; set; }

    public string? ReturnUrl { get; set; }

    public IActionResult OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        return Page();
    }

    public IActionResult OnPost(string? returnUrl = null)
    {
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, Claims.Name, Claims.Role);

        var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        identity.AddClaim(new Claim(Claims.AuthenticationTime, time, ClaimValueTypes.Integer64));

        if (string.Equals(Username, "John", StringComparison.OrdinalIgnoreCase))
        {
            // Profile claims:
            identity.AddClaim(new Claim(Claims.Subject, "7DADB7DB-0637-4446-8626-2781B06A9E20"));
            identity.AddClaim(new Claim(Claims.Name, "John F. Kennedy"));
            identity.AddClaim(new Claim(Claims.Gender, "male"));
            identity.AddClaim(new Claim(Claims.GivenName, "John"));
            identity.AddClaim(new Claim(Claims.MiddleName, "Fitzgerald"));
            identity.AddClaim(new Claim(Claims.FamilyName, "Kennedy"));
            identity.AddClaim(new Claim(Claims.Nickname, "JFK"));
            identity.AddClaim(new Claim(Claims.PreferredUsername, "John"));
            identity.AddClaim(new Claim(Claims.Birthdate, "1917-05-29"));
            identity.AddClaim(new Claim(Claims.Profile,
                "https://www.biography.com/people/john-f-kennedy-9362930"));
            identity.AddClaim(new Claim(Claims.Picture,
                "https://www.biography.com/.image/c_fill%2Ccs_srgb%2Cg_face%2Ch_300%2Cq_80%2Cw_300/MTIwNjA4NjMzODY3ODk2MzMy/john-f-kennedy-9362930-1-402.jpg"));
            identity.AddClaim(new Claim(Claims.Website, "https://www.whitehouse.gov/"));
            identity.AddClaim(new Claim(Claims.Locale, "en-US"));
            identity.AddClaim(new Claim(Claims.Zoneinfo, "America/New York"));
            identity.AddClaim(new Claim(Claims.UpdatedAt, "1483225200", ClaimValueTypes.Integer64));

            // Email claim:
            identity.AddClaim(new Claim(Claims.Email, "john.fitzgerald.kennedy@usa.gov"));

            // Phone claim:
            identity.AddClaim(new Claim(Claims.PhoneNumber, "+1 202-456-1111"));

            // Address claim:
            var address = new JObject
            {
                [Claims.Country] = "United States of America",
                [Claims.Locality] = "Washington",
                [Claims.PostalCode] = "DC 20500",
                [Claims.StreetAddress] = "1600 Pennsylvania Ave NW"
            };

            identity.AddClaim(new Claim(Claims.Address, address.ToString(Formatting.None), JsonClaimValueTypes.Json));
        }

        else if (string.Equals(Username, "Donald", StringComparison.OrdinalIgnoreCase))
        {
            // Profile claims:
            identity.AddClaim(new Claim(Claims.Subject, "95D7BE81-0CFB-4B52-9C92-33A45747FCEF"));
            identity.AddClaim(new Claim(Claims.Name, "Donald J. Trump"));
            identity.AddClaim(new Claim(Claims.Gender, "male"));
            identity.AddClaim(new Claim(Claims.GivenName, "Donald"));
            identity.AddClaim(new Claim(Claims.MiddleName, "John"));
            identity.AddClaim(new Claim(Claims.FamilyName, "Trump"));
            identity.AddClaim(new Claim(Claims.Nickname, "The Donald"));
            identity.AddClaim(new Claim(Claims.PreferredUsername, "Donald"));
            identity.AddClaim(new Claim(Claims.Birthdate, "1946-06-14"));
            identity.AddClaim(new Claim(Claims.Profile,
                "https://www.biography.com/people/donald-trump-9511238"));
            identity.AddClaim(new Claim(Claims.Picture,
                "https://www.biography.com/.image/c_fill%2Ccs_srgb%2Cg_face%2Ch_300%2Cq_80%2Cw_300/MTQxNzI4NTg2OTU1NDk5MDE3/donald_trump_photo_michael_stewartwireimage_gettyimages_169093538_croppedjpg.jpg"));
            identity.AddClaim(new Claim(Claims.Website, "https://www.whitehouse.gov/"));
            identity.AddClaim(new Claim(Claims.Locale, "en-US"));
            identity.AddClaim(new Claim(Claims.Zoneinfo, "America/New York"));
            identity.AddClaim(new Claim(Claims.UpdatedAt, "1483225200", ClaimValueTypes.Integer64));

            // Email claim:
            identity.AddClaim(new Claim(Claims.Email, "donald.john.trump@usa.gov"));

            // Phone claim:
            identity.AddClaim(new Claim(Claims.PhoneNumber, "+1 202-456-1111"));

            // Address claim:
            var address = new JObject
            {
                [Claims.Country] = "United States of America",
                [Claims.Locality] = "Washington",
                [Claims.PostalCode] = "DC 20500",
                [Claims.StreetAddress] = "1600 Pennsylvania Ave NW"
            };

            identity.AddClaim(new Claim(Claims.Address, address.ToString(Formatting.None), JsonClaimValueTypes.Json));
        }

        else
        {
            return BadRequest();
        }

        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : "/connect/signin"
        };

        return SignIn(new ClaimsPrincipal(identity), properties, CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
