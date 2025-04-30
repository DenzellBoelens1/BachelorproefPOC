using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Webshop.Shared.DTOs;
using Webshop.Client.Layout;
using Webshop.Client.Services.REST;
using Webshop.Client.Services.GraphQL;
using Webshop.Client.Services.SignalR;
using Webshop.Client.Services.Websockets;

namespace Webshop.Client.Pages
{
    public partial class ProductDetail : ComponentBase, IDisposable
    {
        [Inject] private ProductRestService RestService { get; set; } = default!;
        [Inject] private ProductGraphQLService GraphQLService { get; set; } = default!;
        [Inject] private ProductSignalRService SignalRService { get; set; } = default!;
        [Inject] private ProductWebSocketService WebSocketService { get; set; } = default!;
        [Inject] private AppState AppState { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;

        [Parameter] public int id { get; set; }

        protected ProductDTO.Details? product;
        protected bool isLoading = true;

        protected int quantity = 1;
        protected string customText = string.Empty;
        protected Dictionary<string, string> selectedOptions = new();
        protected bool isCustomTextEnabled = false;

        protected bool customEnabled;
        protected int maxLength;
        protected decimal pricePerChar;

        protected bool CanAddToCart =>
            product != null
            && quantity >= 1
            && quantity <= product.InStock;

        protected override async Task OnInitializedAsync()
        {
            await LoadProduct();
        }

        private async Task LoadProduct()
        {
            isLoading = true;
            product = await RestService.GetProductDetails(id);

            if (product != null)
            {
                var cg = product.Options
                                .Where(o => o.OptionType == "CustomText")
                                .ToList();
                customEnabled = cg.Any();
                maxLength = cg
                    .Where(o => o.OptionValue.StartsWith("MaxLength=", StringComparison.OrdinalIgnoreCase))
                    .Select(o => int.TryParse(o.OptionValue.Split('=')[1], out var m) ? m : 0)
                    .FirstOrDefault();
                pricePerChar = cg
                    .Where(o => o.OptionValue.StartsWith("PricePerCharacter=", StringComparison.OrdinalIgnoreCase))
                    .Select(o => decimal.TryParse(
                        o.OptionValue.Split('=')[1],
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out var c) ? c : 0m)
                    .FirstOrDefault();

                foreach (var grp in product.Options
                                         .Where(o => o.OptionType != "CustomText")
                                         .Select(o => o.OptionType)
                                         .Distinct())
                {
                    selectedOptions[grp] = string.Empty;
                }
            }

            isLoading = false;
        }

        protected async Task AddToCart()
        {
            if (!CanAddToCart || product == null)
                return;

            // Bouw parameters voor prijsberekening
            var ids = new List<int>();
            var values = new Dictionary<int, string>();

            if (isCustomTextEnabled && !string.IsNullOrWhiteSpace(customText))
            {
                var textOpt = product.Options.First(o => o.OptionType == "CustomText");
                ids.Add(textOpt.OptionID);
                values[textOpt.OptionID] = customText;
            }

            foreach (var kv in selectedOptions)
            {
                if (int.TryParse(kv.Value, out var optId))
                {
                    ids.Add(optId);
                    values[optId] = product.Options.First(o => o.OptionID == optId).OptionValue;
                }
            }

            // Kies de juiste service op basis van AppState.SelectedMethod
            PriceDTO priceDto;
            switch (AppState.SelectedMethod)
            {
                case "rest":
                    priceDto = await RestService.CalculatePrice(
                        product.ProductID, quantity, ids, values,
                        isCustomTextEnabled ? customText : null);
                    break;

                case "graphql":
                    priceDto = await GraphQLService.CalculatePrice(
                        product.ProductID, quantity, ids, values,
                        isCustomTextEnabled ? customText : null);
                    break;

                case "signalr":
                    await SignalRService.StartConnectionAsync();
                    priceDto = await SignalRService.CalculatePrice(
                        product.ProductID, quantity, ids, values,
                        isCustomTextEnabled ? customText : null);
                    await SignalRService.StopConnectionAsync();
                    break;

                case "websocket":
                    priceDto = await WebSocketService.CalculatePrice(
                        product.ProductID, quantity, ids, values,
                        isCustomTextEnabled ? customText : null);
                    break;

                default:
                    throw new InvalidOperationException("Ongeselecteerde technologie");
            }

            // Voeg toe aan winkelmandje met door backend berekende totaalprijs
            AppState.AddToCart(
                product.ProductID,
                quantity,
                priceDto.TotalPrice,
                ids,
                values);

            Navigation.NavigateTo("/cart");
        }

        public void Dispose()
        {
            // eventueel SignalR-verbinding netjes afsluiten
            _ = SignalRService.StopConnectionAsync();
        }
    }
}