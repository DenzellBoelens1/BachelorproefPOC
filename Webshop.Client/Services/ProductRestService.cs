using System.Net.Http;
using System.Net.Http.Json;
using Webshop.Shared.DTOs;

namespace Webshop.Client.Services
{
    public class ProductRestService
    {
        private readonly HttpClient _http;

        public ProductRestService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ProductDTO.Index>> GetProducts(int page, int pageSize, string? search = null)
        {
            var query = $"products?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(search))
                query += $"&search={Uri.EscapeDataString(search)}";

            return await _http.GetFromJsonAsync<List<ProductDTO.Index>>(query)
                ?? new List<ProductDTO.Index>();
        }
        public async Task<ProductDTO.Details?> GetProductDetails(int id)
        {
            var response = await _http.GetAsync($"api/products/details/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ProductDTO.Details>();
        }

        public async Task<ProductDTO.Index?> GetProduct(int id)
        {
            var response = await _http.GetAsync($"api/products/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ProductDTO.Index>();
        }

        public async Task<ProductDTO.Index?> UpdateStock(ProductDTO.UpdateStock update)
        {
            var response = await _http.PutAsJsonAsync($"api/products/{update.ProductID}/stock", update);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Voorraad aanpassen mislukt: {response.StatusCode}");

            return await response.Content.ReadFromJsonAsync<ProductDTO.Index>();
        }
    }
}
