using Microsoft.AspNetCore.Components;
using Webshop.Shared.DTOs;
using Webshop.Client.Services;
using Webshop.Client.Layout;

namespace Webshop.Client.Pages
{
    public partial class ProductList : ComponentBase, IDisposable
    {
        [Inject] public AppState AppState { get; set; } = default!;
        [Inject] public ProductRestService RestService { get; set; } = default!;
        [Inject] public ProductGraphQLService GraphQLService { get; set; } = default!;
        [Inject] public ProductSignalRService SignalRService { get; set; } = default!;
        [Inject] public ProductWebSocketService WebSocketService { get; set; } = default!;

        List<ProductDTO.Index>? products;
        int currentPage = 1;
        int _pageSize = 10;
        public int pageSize
        {
            get => _pageSize;
            set
            {
                if (_pageSize != value)
                {
                    _pageSize = value;
                    currentPage = 1;
                    lastGraphQLCursor = null;
                    previousCursors.Clear();
                    _ = LoadProducts(); // "fire and forget" async call
                }
            }
        }
        string selectedMethod = "rest";
        bool isLoading = false;

        string? lastGraphQLCursor = null;
        Stack<string?> previousCursors = new();

        protected override async Task OnInitializedAsync()
        {
            AppState.OnMethodChanged += HandleMethodChanged;
            selectedMethod = AppState.SelectedMethod;

            SignalRService.OnProductsReceived += result =>
            {
                products = result;
                isLoading = false;
                StateHasChanged();
            };

            await LoadProducts();
        }

        private async void HandleMethodChanged()
        {
            selectedMethod = AppState.SelectedMethod;
            currentPage = 1;
            lastGraphQLCursor = null;
            previousCursors.Clear();
            await LoadProducts();
        }

        async Task LoadProducts()
        {
            isLoading = true;
            StateHasChanged();

            if (selectedMethod == "rest")
            {
                products = await RestService.GetProducts(currentPage, pageSize);
            }
            else if (selectedMethod == "graphql")
            {
                products = await GraphQLService.GetProductsGraphQL(pageSize, lastGraphQLCursor);
                if (GraphQLService.HasNextPage)
                {
                    previousCursors.Push(lastGraphQLCursor);
                    lastGraphQLCursor = GraphQLService.LastCursor;
                }
            }
            else if (selectedMethod == "signalr")
            {
                await SignalRService.StartConnectionAsync();
                await SignalRService.RequestProducts(currentPage, pageSize);
            }
            else if (selectedMethod == "websocket")
            {
                products = await WebSocketService.GetProducts(currentPage, pageSize);
            }

            isLoading = false;
            StateHasChanged();
        }

        async Task NextPage()
        {
            if (selectedMethod == "graphql")
            {
                currentPage++;
                await LoadProducts();
            }
            else
            {
                currentPage++;
                await LoadProducts();
            }
        }

        async Task PreviousPage()
        {
            if (selectedMethod == "graphql")
            {
                if (previousCursors.Count > 1)
                {
                    previousCursors.Pop();
                    lastGraphQLCursor = previousCursors.Peek();
                    await LoadProducts();
                }
            }
            else if (currentPage > 1)
            {
                currentPage--;
                await LoadProducts();
            }
        }

        public void Dispose()
        {
            AppState.OnMethodChanged -= HandleMethodChanged;
        }
    }
}
