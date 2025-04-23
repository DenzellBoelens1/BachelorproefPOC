using System.Security.Claims;

namespace Webshop.Client.Pages
{
    public partial class Profile
    {
        private string? userName;
        private string? email;
        private string? userId;
        private bool isAuthenticated = false;
        private bool isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated != true)
            {
                isAuthenticated = false;
                Navigation.NavigateTo("/login", true);
                return;
            }

            isAuthenticated = true;
            userName = user.Identity?.Name;
            email = user.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
            userId = user.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            isLoading = false;
        }

        private void GaNaarBestellingen()
        {
            Navigation.NavigateTo("/orders");
        }
    }
}