using System.Net.Http.Json;
using System.Text.Json;
using Webshop.Shared.DTOs;

namespace Webshop.Client.Services.GraphQL
{
    public class OrderGraphQLService
    {
        private readonly HttpClient _http;

        public OrderGraphQLService(HttpClient http)
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
            {
                throw new Exception("GraphQL response does not contain 'data'.");
            }

            return data;
        }

        public async Task<List<OrderDTO>> GetOrdersByUser(int userId)
        {
            var query = @"
                query($userId: Int!) {
                    getOrdersByUser(userId: $userId) {
                        orderID
                        orderDate
                        totalPrice
                        items {
                            productID
                            quantity
                            price
                            options {
                                optionID
                                optionType
                                optionValue
                            }
                        }
                    }
                }";

            var variables = new { userId };
            var data = await SendGraphQLRequestAsync(query, variables);

            var ordersElement = data.GetProperty("getOrdersByUser");

            var orders = new List<OrderDTO>();

            foreach (var orderEl in ordersElement.EnumerateArray())
            {
                var order = new OrderDTO
                {
                    OrderID = orderEl.GetProperty("orderID").GetInt32(),
                    OrderDate = orderEl.GetProperty("orderDate").GetDateTime(),
                    TotalPrice = orderEl.GetProperty("totalPrice").GetDecimal(),
                    Items = new List<OrderItemDTO>()
                };

                foreach (var itemEl in orderEl.GetProperty("items").EnumerateArray())
                {
                    var item = new OrderItemDTO
                    {
                        ProductID = itemEl.GetProperty("productID").GetInt32(),
                        Quantity = itemEl.GetProperty("quantity").GetInt32(),
                        Price = itemEl.GetProperty("price").GetDecimal(),
                        Options = new List<OrderItemOptionDTO>()
                    };

                    foreach (var opt in itemEl.GetProperty("options").EnumerateArray())
                    {
                        item.Options.Add(new OrderItemOptionDTO
                        {
                            OptionID = opt.GetProperty("optionID").GetInt32(),
                            OptionType = opt.GetProperty("optionType").GetString() ?? string.Empty,
                            OptionValue = opt.GetProperty("optionValue").GetString() ?? string.Empty
                        });
                    }

                    order.Items.Add(item);
                }

                orders.Add(order);
            }

            return orders;
        }
    }
}
