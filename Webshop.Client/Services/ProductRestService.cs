using System.Net.Http.Json;
using Webshop.Shared.DTOs;
using Webshop.Shared.Models;


namespace Webshop.Client.Services
{
    public class ProductRestService
    {
        private readonly HttpClient _http;
        public ProductRestService(HttpClient http) => _http = http;

        public async Task<List<ProductDTO.Index>?> GetProducts(int page, int pageSize)
        {
            return await _http.GetFromJsonAsync<List<ProductDTO.Index>>($"api/products?page={page}&pageSize={pageSize}");
        }

        public async Task<ProductDTO.Index> GetProduct(int id)
        {
            return await _http.GetFromJsonAsync<ProductDTO.Index>($"api/products/{id}");
        }
    }

}
