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

        public async Task<List<OrderDTO.Index>> GetOrdersByUser(int userId)
        {
            var response = await _http.GetAsync($"api/orders/user/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Fout bij ophalen van bestellingen: {response.StatusCode} - {msg}");
            }

            var orders = await response.Content.ReadFromJsonAsync<List<OrderDTO.Index>>();
            return orders ?? new List<OrderDTO.Index>();
        }

        public async Task<OrderDTO.Created> PlaceOrder(OrderDTO.Create orderDto)
        {
            var response = await _http.PostAsJsonAsync("api/orders", orderDto);

            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Fout bij plaatsen van bestelling: {response.StatusCode} - {msg}");
            }

            var createdOrder = await response.Content.ReadFromJsonAsync<OrderDTO.Created>();
            return createdOrder ?? throw new Exception("Er is een fout opgetreden bij het verwerken van de bestelling.");
        }
    }
}
