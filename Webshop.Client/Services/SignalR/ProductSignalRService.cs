using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webshop.Shared.DTOs;

namespace Webshop.Client.Services.SignalR
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
                .WithUrl("http://localhost:5139/signalr/product", options =>
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

        public async Task RequestProductById(int id)
        {
            if (await CheckConnectionAsync())
            {
                await _connection!.InvokeAsync("GetProductById", id);
            }
        }

        public async Task RequestProductDetailsById(int id)
        {
            if (await CheckConnectionAsync())
            {
                await _connection!.InvokeAsync("GetProductDetails", id);
            }
        }

        public async Task UpdateStock(ProductDTO.UpdateStock update)
        {
            if (await CheckConnectionAsync())
            {
                await _connection!.InvokeAsync("UpdateStock", update.ProductID, update.InStock);
            }
        }

        /// <summary>
        /// Calls the backend CalculatePrice method via SignalR and returns PriceDTO
        /// </summary>
        public async Task<PriceDTO> CalculatePrice(
            int productId,
            int quantity,
            List<int> selectedOptionIds,
            Dictionary<int, string> optionValues,
            string? customText)
        {
            if (!await CheckConnectionAsync())
                throw new InvalidOperationException("SignalR connection not established.");

            // Set up TaskCompletionSource
            var tcs = new TaskCompletionSource<PriceDTO>();
            _connection.On<PriceDTO>("ReceivePriceCalculation", dto =>
            {
                tcs.TrySetResult(dto);
            });

            await _connection.InvokeAsync("CalculatePrice",
                productId, quantity, selectedOptionIds, optionValues, customText);

            // Wacht op de callback
            return await tcs.Task;
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