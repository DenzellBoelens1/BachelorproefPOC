using System.Globalization;
using Microsoft.AspNetCore.Components;
using Webshop.Shared.DTOs;
using Webshop.Client.Layout;
using Webshop.Client.Services.GraphQL;
using Webshop.Client.Services.REST;
using Webshop.Client.Services.SignalR;
using Webshop.Client.Services.Websockets;

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

        protected ProductDTO.Details? product;
        protected bool isLoading = true;
        protected int quantity = 1;
        protected string customText = string.Empty;
        protected Dictionary<string, string> selectedOptions = new();
        protected bool isCustomTextEnabled = false;

        // CustomText‐velden
        protected bool customEnabled;
        protected int maxLength;
        protected decimal pricePerChar;

        // Button‐enable property
        protected bool CanAddToCart =>
            product != null
            && quantity >= 1
            && quantity <= product.InStock;

        protected override async Task OnInitializedAsync()
        {
            AppState.OnMethodChanged += HandleMethodChanged;
            SignalRService.OnProductDetailsReceived += OnSignalRProductReceived;
            await LoadProduct();
        }

        private async void HandleMethodChanged() => await LoadProduct();

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
                    return; // wacht op signalR callback
            }

            InitializeCustomTextSettings();
            isLoading = false;
            StateHasChanged();
        }

        void OnSignalRProductReceived(ProductDTO.Details p)
        {
            product = p;
            InitializeCustomTextSettings();
            isLoading = false;
            InvokeAsync(StateHasChanged);
        }

        private void InitializeCustomTextSettings()
        {
            if (product == null) return;
            var customGroup = product.Options
                                    .Where(o => o.OptionType == "CustomText")
                                    .ToList();

            customEnabled = customGroup.Any(o => o.OptionValue.StartsWith("Enabled="));

            maxLength = customGroup
                .Where(o => o.OptionValue.StartsWith("MaxLength="))
                .Select(o => int.TryParse(o.OptionValue.Split('=')[1], out var m) ? m : 0)
                .FirstOrDefault();

            pricePerChar = customGroup
                .Where(o => o.OptionValue.StartsWith("PricePerCharacter="))
                .Select(o => decimal.TryParse(
                        o.OptionValue.Split('=')[1],
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out var c) ? c : 0m)
                .FirstOrDefault();
        }

        async Task AddToCart()
        {
            if (!CanAddToCart || product == null)
                return;

            // Basisprijs + custom-tekst
            decimal totalUnitPrice = product.BasePrice;
            if (isCustomTextEnabled && !string.IsNullOrWhiteSpace(customText))
            {
                totalUnitPrice += customText.Length * pricePerChar;
            }

            // Optie-ID’s en values
            var selectedOptionIds = new List<int>();
            var optionValuesMap = new Dictionary<int, string>();

            // CustomText koppelen: probeer “Input”, anders “Enabled=true”
            if (isCustomTextEnabled && !string.IsNullOrWhiteSpace(customText))
            {
                var textOpt = product.Options
                    .FirstOrDefault(o => o.OptionType == "CustomText" && o.OptionValue == "Input")
                    // geen Input? val terug op Enabled=true
                    ?? product.Options
                       .FirstOrDefault(o => o.OptionType == "CustomText" && o.OptionValue.StartsWith("Enabled="));

                if (textOpt != null)
                {
                    selectedOptionIds.Add(textOpt.OptionID);
                    optionValuesMap[textOpt.OptionID] = customText;
                }
                else
                {
                    Console.Error.WriteLine("Geen CustomText-optie (Input of Enabled) gevonden.");
                }
            }

            // Overige drop-downs
            foreach (var grp in product.Options
                                      .Where(o => o.OptionType != "CustomText")
                                      .GroupBy(o => o.OptionType))
            {
                if (selectedOptions.TryGetValue(grp.Key, out var sel)
                    && int.TryParse(sel, out var optId))
                {
                    var chosen = grp.First(o => o.OptionID == optId);
                    selectedOptionIds.Add(chosen.OptionID);
                    optionValuesMap[chosen.OptionID] = chosen.OptionValue;
                }
            }

            AppState.AddToCart(
                product.ProductID,
                quantity,
                totalUnitPrice,
                selectedOptionIds,
                optionValuesMap);

            StateHasChanged();
        }

        public void Dispose()
        {
            AppState.OnMethodChanged -= HandleMethodChanged;
            SignalRService.OnProductDetailsReceived -= OnSignalRProductReceived;
        }
    }
}
