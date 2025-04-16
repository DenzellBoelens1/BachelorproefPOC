using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Webshop.Shared.DTOs;

namespace Webshop.Client.Services.SignalR
{
    public class OrderSignalRService
    {
        private HubConnection? _connection;
        private readonly NavigationManager _nav;
        private bool _eventsRegistered = false;

        public event Action<List<OrderDTO>>? OnOrdersReceived;

        public OrderSignalRService(NavigationManager nav)
        {
            _nav = nav;
        }

        public async Task StartConnectionAsync()
        {
            if (_connection is not null && _connection.State != HubConnectionState.Disconnected)
                return;

            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5139/signalr/order", options =>
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

            _connection.On<List<OrderDTO>>("ReceiveOrders", orders =>
            {
                OnOrdersReceived?.Invoke(orders);
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

        public async Task RequestOrdersByUser(int userId)
        {
            if (await CheckConnectionAsync())
            {
                await _connection!.InvokeAsync("GetOrdersByUser", userId);
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
