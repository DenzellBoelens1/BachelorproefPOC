using Microsoft.JSInterop;

namespace Webshop.Client.Auth
{
    public class TokenService
    {
        private readonly IJSRuntime _js;

        public TokenService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task SetToken(string token) =>
            await _js.InvokeVoidAsync("localStorage.setItem", "jwt", token);

        public async Task<string?> GetToken() =>
            await _js.InvokeAsync<string>("localStorage.getItem", "jwt");

        public async Task RemoveToken() =>
            await _js.InvokeVoidAsync("localStorage.removeItem", "jwt");
    }

}
