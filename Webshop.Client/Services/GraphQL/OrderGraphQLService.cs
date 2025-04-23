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
                throw new Exception("GraphQL response does not contain 'data'.");

            return data;
        }

        public async Task<List<OrderDTO.Index>> GetOrdersByUser(int userId)
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
                }
            ";

            var data = await SendGraphQLRequestAsync(query, new { userId });
            var ordersEl = data.GetProperty("getOrdersByUser");

            var list = new List<OrderDTO.Index>();
            foreach (var el in ordersEl.EnumerateArray())
            {
                var order = new OrderDTO.Index
                {
                    OrderID = el.GetProperty("orderID").GetInt32(),
                    OrderDate = el.GetProperty("orderDate").GetDateTime(),
                    TotalPrice = el.GetProperty("totalPrice").GetDecimal(),
                    Items = new List<OrderItemDTO.Index>()
                };

                foreach (var itemEl in el.GetProperty("items").EnumerateArray())
                {
                    var item = new OrderItemDTO.Index
                    {
                        ProductID = itemEl.GetProperty("productID").GetInt32(),
                        Quantity = itemEl.GetProperty("quantity").GetInt32(),
                        Price = itemEl.GetProperty("price").GetDecimal(),
                        Options = new List<OrderItemOptionDTO.Index>()
                    };

                    foreach (var opt in itemEl.GetProperty("options").EnumerateArray())
                    {
                        item.Options.Add(new OrderItemOptionDTO.Index
                        {
                            OptionID = opt.GetProperty("optionID").GetInt32(),
                            OptionType = opt.GetProperty("optionType").GetString() ?? "",
                            OptionValue = opt.GetProperty("optionValue").GetString() ?? ""
                        });
                    }

                    order.Items.Add(item);
                }

                list.Add(order);
            }

            return list;
        }

        public async Task<OrderDTO.Created> PlaceOrder(OrderDTO.Create orderDto)
        {
            var mutation = @"
                mutation($orderDto: OrderCreateInput!) {
                  placeOrder(orderDto: $orderDto) {
                    orderID
                  }
                }
            ";

            // Let op: de GraphQL‑typename van je input is nog steeds 'OrderCreateInput'
            var data = await SendGraphQLRequestAsync(mutation, new { orderDto });
            var createdEl = data.GetProperty("placeOrder");
            var orderId = createdEl.GetProperty("orderID").GetInt32();

            return new OrderDTO.Created { OrderID = orderId };
        }
    }
}
