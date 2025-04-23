using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Webshop.Client.Layout;
using Webshop.Client.Services.GraphQL;
using Webshop.Client.Services.REST;
using Webshop.Client.Services.SignalR;      // SignalR
using Webshop.Client.Services.Websockets;
using Webshop.Shared.DTOs;
using Webshop.Shared.Models;

namespace Webshop.Client.Pages
{
    public partial class Cart : ComponentBase, IDisposable
    {
        [Inject] public AppState AppState { get; set; } = default!;
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Inject] public ProductRestService RestService { get; set; } = default!;
        [Inject] public ProductGraphQLService GraphQLService { get; set; } = default!;
        [Inject] public ProductSignalRService SignalRService { get; set; } = default!;
        [Inject] public ProductWebSocketService WebSocketService { get; set; } = default!;
        [Inject] public OrderRestService OrderRestService { get; set; } = default!;
        [Inject] public OrderGraphQLService OrderGraphQLService { get; set; } = default!;
        [Inject] public OrderSignalRService OrderSignalRService { get; set; } = default!;
        [Inject] public OrderWebSocketService OrderWebSocketService { get; set; } = default!;

        private List<CartItem> cartItems = new();
        private bool isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            // Subscribe on product details for SignalR
            SignalRService.OnProductDetailsReceived += HandleSignalRProduct;
            await LoadCartItems();
        }

        private void HandleSignalRProduct(ProductDTO.Details product)
        {
            // Add any matching cart entries for this product
            var toAdd = AppState.Cart
                .Where(kv => kv.Key.ProductId == product.ProductID)
                .Select(kv => ToCartItem(
                    kv.Key,
                    kv.Value,
                    product.Name,
                    product.InStock,
                    product.Options));

            cartItems.AddRange(toAdd);
            InvokeAsync(StateHasChanged);
        }

        private async Task LoadCartItems()
        {
            isLoading = true;
            StateHasChanged();

            cartItems.Clear();
            var method = AppState.SelectedMethod;

            if (method == "signalr")
            {
                // Start SignalR connection and request details
                await SignalRService.StartConnectionAsync();
                foreach (var kv in AppState.Cart)
                {
                    await SignalRService.RequestProductDetailsById(kv.Key.ProductId);
                }
                // Items will be added in HandleSignalRProduct
            }
            else
            {
                // REST / GraphQL / WebSocket modes
                foreach (var kv in AppState.Cart)
                {
                    ProductDTO.Details? product = method switch
                    {
                        "rest" => await RestService.GetProductDetails(kv.Key.ProductId),
                        "graphql" => await GraphQLService.GetProductDetailsById(kv.Key.ProductId),
                        "websocket" => await WebSocketService.GetProductDetailsById(kv.Key.ProductId),
                        _ => null
                    };
                    if (product != null)
                    {
                        cartItems.Add(ToCartItem(
                            kv.Key,
                            kv.Value,
                            product.Name,
                            product.InStock,
                            product.Options));
                    }
                }
            }

            isLoading = false;
            StateHasChanged();
        }

        private CartItem ToCartItem(
            CartKey key,
            int quantity,
            string name,
            int inStock,
            List<ProductDTO.OptionDetail> productOptions)
        {
            var map = AppState.CartOptionValues[key];
            var optionsList = map.Select(kv =>
            {
                var meta = productOptions.FirstOrDefault(o => o.OptionID == kv.Key);
                return new OrderItemOptionDTO.Index
                {
                    OptionID = kv.Key,
                    OptionType = meta?.OptionType ?? "Onbekend",
                    OptionValue = kv.Value
                };
            }).ToList();

            var desc = string.Join(", ", optionsList.Select(o => $"{o.OptionType}: {o.OptionValue}"));

            return new CartItem
            {
                Key = key,
                ProductID = key.ProductId,
                Name = name,
                Quantity = quantity,
                InStock = inStock,
                UnitPrice = AppState.CartPrices[key],
                Description = desc,
                Options = optionsList
            };
        }

        private async Task PlaceOrder()
        {
            if (!cartItems.Any()) return;

            // Authentication and claim retrieval
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                Navigation.NavigateTo("/login");
                return;
            }
            var claim = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                        ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out var userId))
            {
                Console.Error.WriteLine("Geen geldige userId-claim gevonden.");
                return;
            }

            // Build DTO
            var dto = new OrderDTO.Create
            {
                UserID = userId,
                OrderDate = DateTime.UtcNow,
                Items = cartItems.Select(ci => new OrderItemDTO.OrderItemCreate
                {
                    ProductID = ci.Key.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    Options = ci.Options.Select(o => new OrderItemOptionDTO.OrderItemOptionCreate
                    {
                        OptionID = o.OptionID,
                        Key = o.OptionType,
                        Value = o.OptionValue
                    }).ToList()
                }).ToList()
            };

            // Place order
            switch (AppState.SelectedMethod)
            {
                case "rest": await OrderRestService.PlaceOrder(dto); break;
                case "graphql": await OrderGraphQLService.PlaceOrder(dto); break;
                case "signalr": await OrderSignalRService.PlaceOrder(dto); break;
                case "websocket": await OrderWebSocketService.PlaceOrder(dto); break;
            }

            AppState.ClearCart();
            Navigation.NavigateTo("/");
        }

        private void GoBack() => Navigation.NavigateTo("/");

        private void RemoveFromCart(CartKey key)
        {
            AppState.RemoveFromCart(key);
            _ = LoadCartItems();
        }

        public void Dispose()
        {
            SignalRService.OnProductDetailsReceived -= HandleSignalRProduct;
        }

        private class CartItem
        {
            public CartKey Key { get; set; } = default!;
            public int ProductID { get; set; }
            public string Name { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public int InStock { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice => Quantity * UnitPrice;
            public string Description { get; set; } = string.Empty;
            public List<OrderItemOptionDTO.Index> Options { get; set; } = new();
        }
    }
}
