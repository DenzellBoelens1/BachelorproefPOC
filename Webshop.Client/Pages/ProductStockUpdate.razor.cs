using Microsoft.AspNetCore.Components;
using Webshop.Client.Layout;
using Webshop.Client.Services;
using Webshop.Shared.DTOs;

namespace Webshop.Client.Pages
{
    public partial class ProductStockUpdate : IDisposable
    {
        [Inject] public AppState AppState { get; set; } = default!;
        [Inject] public ProductRestService RestService { get; set; } = default!;
        [Inject] public ProductGraphQLService GraphQLService { get; set; } = default!;
        [Inject] public ProductSignalRService SignalRService { get; set; } = default!;
        [Inject] public ProductWebSocketService WebSocketService { get; set; } = default!;

        [Parameter] public int id { get; set; }

        ProductDTO.Index? product;
        int newStock;
        bool isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            SignalRService.OnSingleProductReceived += OnSignalRProductReceived;
            await LoadProduct();
        }

        async Task LoadProduct()
        {
            isLoading = true;
            StateHasChanged();

            var method = AppState.SelectedMethod;

            if (method == "signalr")
            {
                await SignalRService.StartConnectionAsync();
                await SignalRService.RequestProductById(id);
            }
            else
            {
                product = await LoadProductForMethod(method);
                if (product != null)
                {
                    newStock = product.InStock;
                }

                isLoading = false;
                StateHasChanged();
            }
        }

        private async Task<ProductDTO.Index?> LoadProductForMethod(string method)
        {
            return method switch
            {
                "rest" => await RestService.GetProduct(id),
                "graphql" => await GraphQLService.GetProductById(id),
                "websocket" => await WebSocketService.GetProductById(id),
                _ => null
            };
        }

        async Task SubmitUpdate()
        {
            if (product == null) return;

            var updateDto = new ProductDTO.UpdateStock
            {
                ProductID = product.ProductID,
                InStock = newStock
            };

            var method = AppState.SelectedMethod;

            if (method == "signalr")
            {
                await SignalRService.UpdateStock(updateDto);
            }
            else
            {
                product = method switch
                {
                    "rest" => await RestService.UpdateStock(updateDto),
                    "graphql" => await GraphQLService.UpdateStock(updateDto),
                    "websocket" => await WebSocketService.UpdateStock(updateDto),
                    _ => product
                };

                StateHasChanged();
            }
        }

        private void OnSignalRProductReceived(ProductDTO.Index p)
        {
            product = p;
            newStock = p.InStock;
            isLoading = false;
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            SignalRService.OnSingleProductReceived -= OnSignalRProductReceived;
        }
    }
}
