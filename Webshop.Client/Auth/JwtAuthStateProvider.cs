using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;



namespace Webshop.Client.Auth
{
    public class JwtAuthStateProvider : AuthenticationStateProvider
    {
        private readonly TokenService _tokenService;

        public JwtAuthStateProvider(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _tokenService.GetToken();

            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = ParseClaimsFromJwt(token);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
        }

        public void NotifyAuthenticationStateChanged()
        {
            var authState = GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(authState);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = Convert.FromBase64String(payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '='));
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes)!;

            var claims = new List<Claim>();

            foreach (var kvp in keyValuePairs)
            {
                var key = kvp.Key;
                var value = kvp.Value.ToString() ?? "";

                // Normalize known claim types
                if (key == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                    key = ClaimTypes.Name;
                else if (key == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                    key = ClaimTypes.NameIdentifier;

                claims.Add(new Claim(key, value));
            }

            return claims;
        }
    }
}
