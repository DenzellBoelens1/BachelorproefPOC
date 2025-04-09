using Microsoft.AspNetCore.Components;
using Webshop.Shared.DTOs;
using Webshop.Client.Services;
using Webshop.Client.Layout;

namespace Webshop.Client.Pages
{
    public partial class ProductDetail : ComponentBase, IDisposable
    {
        [Inject] public AppState AppState { get; set; } = default!;
        [Inject] public ProductRestService RestService { get; set; } = default!;
        [Inject] public ProductGraphQLService GraphQLService { get; set; } = default!;
        [Inject] public ProductSignalRService SignalRService { get; set; } = default!;
        [Inject] public ProductWebSocketService WebSocketService { get; set; } = default!;

        [Parameter] public int id { get; set; }

        ProductDTO.Details? product;
        bool isLoading = true;
        int quantity = 1;
        string customText = string.Empty;
        Dictionary<string, string> selectedOptions = new();
        private bool isCustomTextEnabled = true;


        bool CanAddToCart => product != null && product.InStock >= quantity;

        protected override async Task OnInitializedAsync()
        {
            AppState.OnMethodChanged += HandleMethodChanged;
            SignalRService.OnProductDetailsReceived += OnSignalRProductReceived;

            await LoadProduct();
        }

        private async void HandleMethodChanged()
        {
            await LoadProduct();
        }

        async Task LoadProduct()
        {
            isLoading = true;
            product = null;
            StateHasChanged();

            var method = AppState.SelectedMethod;

            switch (method)
            {
                case "rest":
                    product = await RestService.GetProductDetails(id);
                    break;

                case "graphql":
                    product = await GraphQLService.GetProductDetailsById(id);
                    break;

                case "websocket":
                    product = await WebSocketService.GetProductDetailsById(id);
                    break;

                case "signalr":
                    await SignalRService.StartConnectionAsync();
                    await SignalRService.RequestProductDetailsById(id);
                    break;
            }

            // Voor alles behalve signalr: we hebben direct een antwoord
            if (method != "signalr")
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        void OnSignalRProductReceived(ProductDTO.Details p)
        {
            product = p;
            isLoading = false;
            InvokeAsync(StateHasChanged);
        }

        async Task AddToCart()
        {
            if (product is null || quantity <= 0 || quantity > product.InStock)
                return;

            decimal totalUnitPrice = product.BasePrice;

            var customTextOption = product.Options.FirstOrDefault(o => o.OptionType == "CustomText");
            if (customTextOption != null)
            {
                var enabled = customTextOption.Values.FirstOrDefault(v => v.StartsWith("Enabled="))?.Split('=')[1] == "true";
                var priceStr = customTextOption.Values.FirstOrDefault(v => v.StartsWith("PricePerCharacter="))?.Split('=')[1];

                if (enabled && isCustomTextEnabled && decimal.TryParse(priceStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var pricePerChar))
                {
                    totalUnitPrice += pricePerChar * customText.Length;
                }
            }

            AppState.AddToCart(product.ProductID, quantity, totalUnitPrice, new Dictionary<string, string>(selectedOptions), customText);

            product.InStock -= quantity;

            await UpdateProductStock(product.ProductID, product.InStock);

            StateHasChanged();
        }

        private async Task UpdateProductStock(int productId, int newStock)
        {
            var updateDto = new ProductDTO.UpdateStock
            {
                ProductID = productId,
                InStock = newStock
            };

            var method = AppState.SelectedMethod;

            switch (method)
            {
                case "rest":
                    await RestService.UpdateStock(updateDto);
                    break;
                case "graphql":
                    await GraphQLService.UpdateStock(updateDto);
                    break;
                case "signalr":
                    await SignalRService.UpdateStock(updateDto);
                    break;
                case "websocket":
                    await WebSocketService.UpdateStock(updateDto);
                    break;
            }
        }

        public void Dispose()
        {
            AppState.OnMethodChanged -= HandleMethodChanged;
            SignalRService.OnProductDetailsReceived -= OnSignalRProductReceived;
        }
    }
}
