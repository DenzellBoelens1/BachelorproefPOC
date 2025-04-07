using System.Net.Http.Json;
using System.Text.Json;
using Webshop.Shared.DTOs;

namespace Webshop.Client.Services
{
    public class ProductGraphQLService
    {
        private readonly HttpClient _http;

        public ProductGraphQLService(HttpClient http)
        {
            _http = http;
        }

        public string? LastCursor { get; private set; } // reden voor cursor is omdat ik met usepaging werk van hotchocolate
        public bool HasNextPage { get; private set; }

        public async Task<List<ProductDTO.Index>> GetProductsGraphQL(int pageSize, string? cursor = null)
        {
            var afterPart = cursor != null ? $"after: \"{cursor}\"" : "";
            var query = $@"
            {{
                products(first: {pageSize} {(cursor != null ? $", after: \"{cursor}\"" : "")}) {{
                    totalCount
                    pageInfo {{
                        endCursor
                        hasNextPage
                    }}
                    nodes {{
                        productID
                        name
                        inStock
                    }}
                }}
            }}";

            var requestBody = new { query };

            var response = await _http.PostAsJsonAsync("/graphql", requestBody);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"GraphQL request failed: {response.StatusCode}");

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var data = json.GetProperty("data").GetProperty("products");

            // 🔹 Bewaar endCursor en hasNextPage
            var pageInfo = data.GetProperty("pageInfo");
            LastCursor = pageInfo.GetProperty("endCursor").GetString();
            HasNextPage = pageInfo.GetProperty("hasNextPage").GetBoolean();

            var items = data.GetProperty("nodes");
            var result = new List<ProductDTO.Index>();

            foreach (var item in items.EnumerateArray())
            {
                result.Add(new ProductDTO.Index
                {
                    ProductID = item.GetProperty("productID").GetInt32(),
                    Name = item.GetProperty("name").GetString()!,
                    InStock = item.GetProperty("inStock").GetInt32()
                });
            }

            return result;
        }
    }
}
