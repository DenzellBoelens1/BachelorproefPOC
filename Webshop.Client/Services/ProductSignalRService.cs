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
        private bool _eventsRegistered = false;

        public event Action<List<ProductDTO.Index>>? OnProductsReceived;
        public event Action<ProductDTO.Index>? OnSingleProductReceived;
        public event Action<int>? OnProductNotFound;
        public event Action<ProductDTO.Details>? OnProductDetailsReceived;

        public ProductSignalRService(NavigationManager nav)
        {
            _nav = nav;
        }

        public async Task StartConnectionAsync()
        {
            if (_connection is not null && _connection.State != HubConnectionState.Disconnected)
                return;

            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5139/productHub", options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                })
                .WithAutomaticReconnect()
                .Build();

            if (!_eventsRegistered)
            {
                RegisterEvents();
                _eventsRegistered = true;
            }

            await _connection.StartAsync();
        }

        private void RegisterEvents()
        {
            if (_connection == null) return;

            _connection.On<List<ProductDTO.Index>>("ReceiveProducts", products =>
            {
                OnProductsReceived?.Invoke(products);
            });

            _connection.On<ProductDTO.Index>("ReceiveProduct", product =>
            {
                OnSingleProductReceived?.Invoke(product);
            });

            _connection.On<ProductDTO.Details>("ReceiveProductDetails", product =>
            {
                OnProductDetailsReceived?.Invoke(product);
            });

            _connection.On<int>("ProductNotFound", id =>
            {
                OnProductNotFound?.Invoke(id);
            });

            _connection.On<ProductDTO.Index>("ReceiveStockUpdated", product =>
            {
                OnSingleProductReceived?.Invoke(product);
            });
        }

        private async Task<bool> CheckConnectionAsync()
        {
            if (_connection == null)
                return false;

            if (_connection.State != HubConnectionState.Connected)
            {
                try
                {
                    await _connection.StartAsync();
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        public async Task RequestProducts(int page = 1, int pageSize = 10, string? search = null)
        {
            if (await CheckConnectionAsync())
            {
                await _connection!.InvokeAsync("GetProducts", page, pageSize, search);
            }
        }

        public async Task RequestProductDetailsById(int id)
        {
            if (await CheckConnectionAsync())
            {
                await _connection!.InvokeAsync("GetProductDetails", id);
            }
        }

        public async Task RequestProductById(int id)
        {
            if (await CheckConnectionAsync())
            {
                await _connection!.InvokeAsync("GetProductById", id);
            }
        }

        public async Task UpdateStock(ProductDTO.UpdateStock update)
        {
            if (await CheckConnectionAsync())
            {
                await _connection!.InvokeAsync("UpdateStock", update.ProductID, update.InStock);
            }
        }

        public async Task StopConnectionAsync()
        {
            if (_connection != null && _connection.State != HubConnectionState.Disconnected)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
                _connection = null;
                _eventsRegistered = false;
            }
        }
    }
}
