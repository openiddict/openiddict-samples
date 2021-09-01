using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Contruum.Server
{
    public static class Handlers
    {
        public class PopulateUserinfo : IOpenIddictServerHandler<HandleUserinfoRequestContext>
        {
            public ValueTask HandleAsync(HandleUserinfoRequestContext context)
            {
                if (context.Principal!.HasScope(Scopes.Profile))
                {
                    context.GivenName = context.Principal!.GetClaim(Claims.GivenName);
                    context.FamilyName = context.Principal!.GetClaim(Claims.FamilyName);
                    context.BirthDate = context.Principal!.GetClaim(Claims.Birthdate);
                    context.Profile = context.Principal!.GetClaim(Claims.Profile);
                    context.PreferredUsername = context.Principal!.GetClaim(Claims.PreferredUsername);
                    context.Website = context.Principal!.GetClaim(Claims.Website);

                    context.Claims[Claims.Name] = context.Principal!.GetClaim(Claims.Name);
                    context.Claims[Claims.Gender] = context.Principal!.GetClaim(Claims.Gender);
                    context.Claims[Claims.MiddleName] = context.Principal!.GetClaim(Claims.MiddleName);
                    context.Claims[Claims.Nickname] = context.Principal!.GetClaim(Claims.Nickname);
                    context.Claims[Claims.Picture] = context.Principal!.GetClaim(Claims.Picture);
                    context.Claims[Claims.Locale] = context.Principal!.GetClaim(Claims.Locale);
                    context.Claims[Claims.Zoneinfo] = context.Principal!.GetClaim(Claims.Zoneinfo);
                    context.Claims[Claims.UpdatedAt] = long.Parse(
                        context.Principal!.GetClaim(Claims.UpdatedAt)!,
                        NumberStyles.Number, CultureInfo.InvariantCulture);
                }

                if (context.Principal!.HasScope(Scopes.Email))
                {
                    context.Email = context.Principal!.GetClaim(Claims.Email);
                    context.EmailVerified = false;
                }

                if (context.Principal!.HasScope(Scopes.Phone))
                {
                    context.PhoneNumber = context.Principal!.GetClaim(Claims.PhoneNumber);
                    context.PhoneNumberVerified = false;
                }

                if (context.Principal!.HasScope(Scopes.Address))
                {
                    context.Address = JsonSerializer.Deserialize<JsonElement>(context.Principal!.GetClaim(Claims.Address)!);
                }

                return default;
            }
        }
    }
}
