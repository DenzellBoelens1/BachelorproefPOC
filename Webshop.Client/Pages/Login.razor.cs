using System.Net.Http.Json;
using Webshop.Client.Auth;

namespace Webshop.Client.Pages
{
    public partial class Login
    {
        string username = "";
        string password = "";
        string errorMessage = "";
        bool isSubmitting = false;

        bool showPassword = false;
        string passwordInputType => showPassword ? "text" : "password";

        void TogglePasswordVisibility()
        {
            showPassword = !showPassword;
        }

        async Task HandleLogin()
        {
            errorMessage = "";
            isSubmitting = true;

            try
            {
                var result = await Http.PostAsJsonAsync("api/auth/login", new { Username = username, Password = password });

                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    var token = content["token"];

                    await TokenService.SetToken(token);

                    AuthStateProvider.NotifyAuthenticationStateChanged();

                    Nav.NavigateTo("/", forceLoad: true);
                }
                else
                {
                    errorMessage = "Ongeldige gebruikersnaam of wachtwoord.";
                }
            }
            catch
            {
                errorMessage = "Er ging iets mis tijdens het inloggen.";
            }

            isSubmitting = false;
        }


    }
}