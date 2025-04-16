using System.Net.Http;
using System.Net.Http.Json;
using Webshop.Shared.DTOs;

namespace Webshop.Client.Services.REST
{
    public class OrderRestService
    {
        private readonly HttpClient _http;

        public OrderRestService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<OrderDTO>> GetOrdersByUser(int userId)
        {
            var response = await _http.GetAsync($"api/orders/user/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Fout bij ophalen van bestellingen: {response.StatusCode} - {msg}");
            }

            var orders = await response.Content.ReadFromJsonAsync<List<OrderDTO>>();
            return orders ?? new List<OrderDTO>();
        }
    }
}
