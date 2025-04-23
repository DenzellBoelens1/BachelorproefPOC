using System.Security.Claims;
using Webshop.Client.Layout;
using Webshop.Shared.DTOs;

namespace Webshop.Client.Pages
{
    public partial class Orders
    {
        private List<OrderDTO.Index> orders = new();
        private bool isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            AppState.OnMethodChanged += OnMethodChanged;

            var authState = await AuthProvider.GetAuthenticationStateAsync();
            var userIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                isLoading = false;
                return;
            }

            await LoadOrders(userId);
        }

        private async Task LoadOrders(int userId)
        {
            isLoading = true;
            StateHasChanged();

            switch (AppState.SelectedMethod)
            {
                case "rest":
                    orders = await RestService.GetOrdersByUser(userId);
                    break;

                case "graphql":
                    orders = await GraphQLService.GetOrdersByUser(userId);
                    break;

                case "signalr":
                    SignalRService.OnOrdersReceived += OnSignalROrdersReceived;
                    await SignalRService.StartConnectionAsync();
                    await SignalRService.RequestOrdersByUser(userId);
                    return; // we wachten op event

                case "websocket":
                    orders = await WebSocketService.GetOrdersByUser(userId);
                    break;
            }

            isLoading = false;
            StateHasChanged();
        }

        private void OnSignalROrdersReceived(List<OrderDTO.Index> receivedOrders)
        {
            orders = receivedOrders;
            isLoading = false;
            SignalRService.OnOrdersReceived -= OnSignalROrdersReceived;
            InvokeAsync(StateHasChanged);
        }

        private async void OnMethodChanged()
        {
            var authState = await AuthProvider.GetAuthenticationStateAsync();
            var userIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                await LoadOrders(userId);
            }
        }

        public void Dispose()
        {
            AppState.OnMethodChanged -= OnMethodChanged;
            SignalRService.OnOrdersReceived -= OnSignalROrdersReceived;
        }
    }
}