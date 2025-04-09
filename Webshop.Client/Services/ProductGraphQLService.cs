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

        public string? LastCursor { get; private set; }
        public bool HasNextPage { get; private set; }

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
            {
                throw new Exception("GraphQL response does not contain 'data'.");
            }

            return data;
        }

        public async Task<List<ProductDTO.Index>> GetProductsGraphQL(int pageSize, string? cursor = null, string? searchTerm = null)
        {
            var query = @"
                query ($first: Int!, $after: String, $search: String) {
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
                }";

            var variables = new
            {
                first = pageSize,
                after = cursor,
                search = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm
            };

            var data = await SendGraphQLRequestAsync(query, variables);
            var productsData = data.GetProperty("products");

            // Bewaar paginatie-info
            var pageInfo = productsData.GetProperty("pageInfo");
            LastCursor = pageInfo.GetProperty("endCursor").GetString();
            HasNextPage = pageInfo.GetProperty("hasNextPage").GetBoolean();

            var result = new List<ProductDTO.Index>();
            foreach (var item in productsData.GetProperty("nodes").EnumerateArray())
            {
                result.Add(ParseProduct(item));
            }

            return result;
        }

        public async Task<ProductDTO.Index?> GetProductById(int id)
        {
            var query = @"
                query ($id: Int!) {
                    productById(id: $id) {
                        productID
                        name
                        inStock
                    }
                }";

            var variables = new { id };
            var data = await SendGraphQLRequestAsync(query, variables);

            var productElement = data.GetProperty("productById");
            if (productElement.ValueKind == JsonValueKind.Null) return null;

            return ParseProduct(productElement);
        }

        public async Task<ProductDTO.Details?> GetProductDetailsById(int id)
        {
            var query = @"
        query ($id: Int!) {
            productDetails(id: $id) {
                productID
                name
                description
                basePrice
                inStock
                options {
                    optionType
                    values
                }
            }
        }";

            var variables = new { id };
            var data = await SendGraphQLRequestAsync(query, variables);

            var productElement = data.GetProperty("productDetails");
            if (productElement.ValueKind == JsonValueKind.Null) return null;

            return ParseDetailedProduct(productElement);
        }

        public async Task<ProductDTO.Index?> UpdateStock(ProductDTO.UpdateStock update)
        {
            var mutation = @"
                mutation ($productID: Int!, $inStock: Int!) {
                    updateProductStock(productID: $productID, inStock: $inStock) {
                        productID
                        name
                        inStock
                    }
                }";

            var variables = new { productID = update.ProductID, inStock = update.InStock };
            var data = await SendGraphQLRequestAsync(mutation, variables);

            var updatedProduct = data.GetProperty("updateProductStock");
            return ParseProduct(updatedProduct);
        }

        private static ProductDTO.Index ParseProduct(JsonElement item)
        {
            return new ProductDTO.Index
            {
                ProductID = item.GetProperty("productID").GetInt32(),
                Name = item.GetProperty("name").GetString() ?? string.Empty,
                InStock = item.GetProperty("inStock").GetInt32()
            };
        }

        private static ProductDTO.Details ParseDetailedProduct(JsonElement item)
        {
            var details = new ProductDTO.Details
            {
                ProductID = item.GetProperty("productID").GetInt32(),
                Name = item.GetProperty("name").GetString() ?? string.Empty,
                Description = item.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                BasePrice = item.TryGetProperty("basePrice", out var price) ? price.GetDecimal() : 0,
                InStock = item.GetProperty("inStock").GetInt32(),
                Options = new List<ProductDTO.OptionGroup>()
            };

            if (item.TryGetProperty("options", out var optionsElem) && optionsElem.ValueKind == JsonValueKind.Array)
            {
                foreach (var opt in optionsElem.EnumerateArray())
                {
                    var group = new ProductDTO.OptionGroup
                    {
                        OptionType = opt.GetProperty("optionType").GetString() ?? string.Empty,
                        Values = opt.GetProperty("values").EnumerateArray()
                                    .Select(v => v.GetString() ?? string.Empty)
                                    .ToList()
                    };
                    details.Options.Add(group);
                }
            }

            return details;
        }
    }
}
