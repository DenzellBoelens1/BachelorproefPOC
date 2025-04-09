using Microsoft.AspNetCore.Components;
using Webshop.Client.Layout;
using Webshop.Client.Services;
using Webshop.Shared.DTOs;
using Webshop.Shared.Models;

namespace Webshop.Client.Pages
{
    public partial class Cart
    {
        [Inject] public AppState AppState { get; set; } = default!;
        [Inject] public ProductRestService RestService { get; set; } = default!;
        [Inject] public ProductGraphQLService GraphQLService { get; set; } = default!;
        [Inject] public ProductSignalRService SignalRService { get; set; } = default!;
        [Inject] public ProductWebSocketService WebSocketService { get; set; } = default!;

        private List<CartItem> cartItems = new();
        private bool isLoading = true;
        //public record CartKey(int ProductId, string OptionSignature);
        protected override async Task OnInitializedAsync()
        {
            await LoadCartItems();
        }

        private async Task LoadCartItems()
        {
            isLoading = true;
            StateHasChanged();

            var method = AppState.SelectedMethod;

            if (method == "signalr")
            {
                await SignalRService.StartConnectionAsync();
                cartItems.Clear();

                foreach (var (key, quantity) in AppState.Cart)
                {
                    var productId = key.ProductId;

                    SignalRService.OnSingleProductReceived += product =>
                    {
                        if (product.ProductID == productId)
                        {
                            cartItems.Add(new CartItem
                            {
                                Key = key,
                                ProductID = productId,
                                Name = product.Name,
                                Quantity = quantity,
                                InStock = product.InStock,
                                UnitPrice = AppState.CartPrices[key],
                                Description = AppState.CartDescriptions.TryGetValue(key, out var desc) ? desc : ""
                            });
                            InvokeAsync(StateHasChanged);
                        }
                    };

                    await SignalRService.RequestProductById(productId);

                }

                isLoading = false;
            }
            else
            {
                cartItems = await LoadCartItemsForMethod(method);
                isLoading = false;
                StateHasChanged();
            }
        }
        private async Task<List<CartItem>> LoadCartItemsForMethod(string method)
        {
            var items = new List<CartItem>();

            foreach (var (key, quantity) in AppState.Cart)
            {
                var productId = key.ProductId;
                var product = method switch
                {
                    "rest" => await RestService.GetProduct(productId),
                    "graphql" => await GraphQLService.GetProductById(productId),
                    "websocket" => await WebSocketService.GetProductById(productId),
                    _ => null
                };

                if (product != null)
                {
                    // Voeg nu toe aan de lokale lijst items in plaats van cartItems.
                    items.Add(new CartItem
                    {
                        Key = key,
                        ProductID = productId,
                        Name = product.Name,
                        Quantity = quantity,
                        InStock = product.InStock,
                        UnitPrice = AppState.CartPrices[key],
                        Description = AppState.CartDescriptions.TryGetValue(key, out var desc) ? desc : ""
                    });
                }
            }

            return items;
        }

        private void GoBack()
        {
            Navigation.NavigateTo("/");
        }

        private void RemoveFromCart(CartKey key)
        {
            AppState.RemoveFromCart(key);
            cartItems.RemoveAll(p => p.Key == key);
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
        }
    }
}