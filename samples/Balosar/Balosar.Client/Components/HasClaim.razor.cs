using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Balosar.Client.Components
{
    public partial class HasClaim
    {
        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; }
        [Inject] private HttpClient Http { get; set; }

        [Parameter] public ClaimSide ClaimSide { get; set; }
        [Parameter] public string ClaimType { get; set; }
        [Parameter] public string ClaimValue { get; set; }
        [Parameter] public bool Expected { get; set; }

        private bool UserAuthenticated { get; set; }
        private bool UserHasClaim { get; set; }
        private string ResultBackgrounColor => UserHasClaim ? "#B4E4A4" : "#ECBFBF";
        private MarkupString HasClaimMessage
        {
            get
            {
                if (UserHasClaim)
                {
                    return new MarkupString("<span style='color:green;font-weight:bold'> YES </span>");
                }

                return new MarkupString("<span style='color:red;font-weight:bold'> NO </span>");
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            var user = (await AuthStateProvider.GetAuthenticationStateAsync()).User;
            if (user.Identity != null)
                UserAuthenticated = user.Identity.IsAuthenticated;

            if (UserAuthenticated)
            {
                if (ClaimSide == ClaimSide.Client)
                {
                    UserHasClaim = user.HasClaim(ClaimType, ClaimValue);
                }
                else
                {
                    UserHasClaim = await Http.GetFromJsonAsync<bool>($"/api/claims/has-claim?type={ClaimType}&value={ClaimValue}&stamp={DateTime.Now.Ticks}");
                }
            }
        }
    }
}