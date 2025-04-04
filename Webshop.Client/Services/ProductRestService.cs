using System.Net.Http.Json;
using Webshop.Shared.DTOs;
using Webshop.Shared.Models;


namespace Webshop.Client.Services
{
    public class ProductRestService
    {
        private readonly HttpClient _http;
        public ProductRestService(HttpClient http) => _http = http;

        public async Task<List<ProductDTO>?> GetProducts(int page, int pageSize)
        {
            return await _http.GetFromJsonAsync<List<ProductDTO>>($"api/products?page={page}&pageSize={pageSize}");
        }

        public async Task<Product?> GetProduct(int id)
        {
            return await _http.GetFromJsonAsync<Product>($"api/products/{id}");
        }
    }

}
