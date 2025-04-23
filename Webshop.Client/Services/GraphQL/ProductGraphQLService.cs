using System.Net.Http.Json;
using System.Text.Json;
using Webshop.Shared.DTOs;

namespace Webshop.Client.Services.GraphQL
{
    public class ProductGraphQLService
    {
        private readonly HttpClient _http;
        public string? LastCursor { get; private set; }
        public bool HasNextPage { get; private set; }

        public ProductGraphQLService(HttpClient http)
        {
            _http = http;
        }

        private async Task<JsonElement> SendGraphQLRequestAsync(string query, object? variables = null)
        {
            var requestBody = new { query, variables };
            var response = await _http.PostAsJsonAsync("/graphql", requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"GraphQL request failed ({response.StatusCode}): {content}");
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (!json.TryGetProperty("data", out var data))
                throw new Exception("GraphQL response does not contain 'data'.");

            return data;
        }

        public async Task<List<ProductDTO.Index>> GetProductsGraphQL(int pageSize, string? cursor = null, string? searchTerm = null)
        {
            var query = @"
                query($first: Int!, $after: String, $search: String) {
                  products(first: $first, after: $after, search: $search) {
                    totalCount
                    pageInfo {
                      endCursor
                      hasNextPage
                    }
                    nodes {
                      productID
                      name
                      inStock
                    }
                  }
                }
            ";

            var vars = new
            {
                first = pageSize,
                after = cursor,
                search = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm
            };

            var data = await SendGraphQLRequestAsync(query, vars);
            var productsData = data.GetProperty("products");

            var pageInfo = productsData.GetProperty("pageInfo");
            LastCursor = pageInfo.GetProperty("endCursor").GetString();
            HasNextPage = pageInfo.GetProperty("hasNextPage").GetBoolean();

            var list = new List<ProductDTO.Index>();
            foreach (var node in productsData.GetProperty("nodes").EnumerateArray())
                list.Add(ParseProduct(node));

            return list;
        }

        public async Task<ProductDTO.Index?> GetProductById(int id)
        {
            var query = @"
                query($id: Int!) {
                  productById(id: $id) {
                    productID
                    name
                    inStock
                  }
                }
            ";

            var data = await SendGraphQLRequestAsync(query, new { id });
            var productEl = data.GetProperty("productById");
            if (productEl.ValueKind == JsonValueKind.Null) return null;
            return ParseProduct(productEl);
        }

        public async Task<ProductDTO.Details?> GetProductDetailsById(int id)
        {
            var query = @"
                query($id: Int!) {
                  productDetails(id: $id) {
                    productID
                    name
                    description
                    basePrice
                    inStock
                    options {
                      optionID
                      optionType
                      optionValue
                    }
                  }
                }
            ";

            var data = await SendGraphQLRequestAsync(query, new { id });
            var detailsEl = data.GetProperty("productDetails");
            if (detailsEl.ValueKind == JsonValueKind.Null) return null;
            return ParseDetailedProduct(detailsEl);
        }

        public async Task<ProductDTO.Index?> UpdateStock(ProductDTO.UpdateStock update)
        {
            var mutation = @"
                mutation($productID: Int!, $inStock: Int!) {
                  updateProductStock(productID: $productID, inStock: $inStock) {
                    productID
                    name
                    inStock
                  }
                }
            ";

            var data = await SendGraphQLRequestAsync(mutation, new
            {
                productID = update.ProductID,
                inStock = update.InStock
            });

            var updatedEl = data.GetProperty("updateProductStock");
            return ParseProduct(updatedEl);
        }

        private static ProductDTO.Index ParseProduct(JsonElement e) =>
            new ProductDTO.Index
            {
                ProductID = e.GetProperty("productID").GetInt32(),
                Name = e.GetProperty("name").GetString() ?? "",
                InStock = e.GetProperty("inStock").GetInt32()
            };

        private static ProductDTO.Details ParseDetailedProduct(JsonElement e)
        {
            var d = new ProductDTO.Details
            {
                ProductID = e.GetProperty("productID").GetInt32(),
                Name = e.GetProperty("name").GetString() ?? "",
                Description = e.GetProperty("description").GetString() ?? "",
                BasePrice = e.GetProperty("basePrice").GetDecimal(),
                InStock = e.GetProperty("inStock").GetInt32(),
                Options = new List<ProductDTO.OptionDetail>()
            };

            foreach (var opt in e.GetProperty("options").EnumerateArray())
            {
                d.Options.Add(new ProductDTO.OptionDetail
                {
                    OptionID = opt.GetProperty("optionID").GetInt32(),
                    OptionType = opt.GetProperty("optionType").GetString() ?? "",
                    OptionValue = opt.GetProperty("optionValue").GetString() ?? ""
                });
            }

            return d;
        }
    }
}
