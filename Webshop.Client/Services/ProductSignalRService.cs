using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Webshop.Shared.DTOs;

namespace Webshop.Client.Services
{
    public class ProductSignalRService
    {
        private HubConnection? _connection;
        private readonly NavigationManager _nav;

        public event Action<List<ProductDTO>>? OnProductsReceived;

        public ProductSignalRService(NavigationManager nav)
        {
            _nav = nav;
        }

        public async Task StartConnectionAsync()
        {
            _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5139/productHub", options =>
            {
                options.Transports = HttpTransportType.WebSockets; // 👈 forceer WebSocket
            })
            .WithAutomaticReconnect()
            .Build();

            _connection.On<List<ProductDTO>>("ReceiveProducts", (products) =>
            {
                OnProductsReceived?.Invoke(products);
            });

            await _connection.StartAsync();
        }

        public async Task RequestProducts(int page = 1, int pageSize = 10)
        {
            if (_connection != null && _connection.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("GetProducts", page, pageSize);
            }
        }

    }
}
