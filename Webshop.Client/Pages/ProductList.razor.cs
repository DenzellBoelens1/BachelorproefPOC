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
        string searchTerm = string.Empty;


        public int pageSize
        {
            get => _pageSize;
            set
            {
                if (_pageSize != value)
                {
                    _pageSize = value;
                    currentPage = 1;
                    ResetGraphQLPaging();
                    _ = LoadProducts(); // Fire and forget
                }
            }
        }

        bool isLoading = false;
        string? lastGraphQLCursor = null;
        Stack<string?> previousCursors = new();

        protected override async Task OnInitializedAsync()
        {
            AppState.OnMethodChanged += HandleMethodChanged;
            SignalRService.OnProductsReceived += OnSignalRProductsReceived;

            await LoadProducts();
        }

        private void OnSignalRProductsReceived(List<ProductDTO.Index> result)
        {
            products = result;
            isLoading = false;
            InvokeAsync(StateHasChanged);
        }

        private async void HandleMethodChanged()
        {
            currentPage = 1;
            ResetGraphQLPaging();
            await LoadProducts();
        }

        async Task LoadProducts()
        {
            isLoading = true;
            StateHasChanged();

            var method = AppState.SelectedMethod;

            switch (method)
            {
                case "rest":
                    products = await RestService.GetProducts(currentPage, pageSize, searchTerm);
                    break;

                case "graphql":
                    products = await GraphQLService.GetProductsGraphQL(pageSize, lastGraphQLCursor, searchTerm);
                    if (GraphQLService.HasNextPage)
                    {
                        previousCursors.Push(lastGraphQLCursor);
                        lastGraphQLCursor = GraphQLService.LastCursor;
                    }
                    break;

                case "signalr":
                    await SignalRService.StartConnectionAsync();
                    await SignalRService.RequestProducts(currentPage, pageSize, searchTerm);
                    return;

                case "websocket":
                    products = await WebSocketService.GetProducts(currentPage, pageSize, searchTerm);
                    break;
            }

            isLoading = false;
            StateHasChanged();
        }

        async Task NextPage()
        {
            currentPage++;
            await LoadProducts();
        }

        async Task PreviousPage()
        {
            if (AppState.SelectedMethod == "graphql")
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

        async Task OnSearch()
        {
            currentPage = 1;
            ResetGraphQLPaging();
            await LoadProducts();
        }

        async Task ClearSearch()
        {
            searchTerm = string.Empty;
            currentPage = 1;
            ResetGraphQLPaging();
            await LoadProducts();
        }

        void ResetGraphQLPaging()
        {
            lastGraphQLCursor = null;
            previousCursors.Clear();
        }

        public void Dispose()
        {
            AppState.OnMethodChanged -= HandleMethodChanged;
            SignalRService.OnProductsReceived -= OnSignalRProductsReceived;
        }
    }
}
