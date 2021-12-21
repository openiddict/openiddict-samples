using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Balosar.Client.Components
{
    public partial class ShowClaims
    {
        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; }
        [Inject] private HttpClient Http { get; set; }

        private IEnumerable<Claim> Claims { get; set; }
        private bool IsAuthenticated { get; set; }
        
        private MarkupString AuthenticatedMessage
        {
            get
            {
                if (IsAuthenticated)
                    return new MarkupString("User is <span style='font-weight:bold;color:lime'>authenticated</span>");
                return new MarkupString("User is <span style='font-weight:bold;color:red'>NOT</span> authenticated, please login to see claims");
            }
        }

        public bool ShowAddClaims => Claims.All(m => m.Type != "ENTITY_TYPE_1") && !ShowLogoutLoginMessage;

        public bool ShowLogoutLoginMessage { get; set; } = false;

        protected override async Task OnParametersSetAsync()
        {
            var user = (await AuthStateProvider.GetAuthenticationStateAsync()).User;
            Claims = user.Claims;
            IsAuthenticated = user.Identity?.IsAuthenticated ?? false;
        }

        private async Task AddClaims()
        {
            await Http.PostAsJsonAsync("/api/claims/add-sample", new {});
            ShowLogoutLoginMessage = true;
        }
    }
}
