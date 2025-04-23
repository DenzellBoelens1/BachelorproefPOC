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

        public event Action<List<OrderDTO.Index>>? OnOrdersReceived;
        public event Action<OrderDTO.Created>? OnOrderPlaced;

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

            // **Extra logging en reconnect-events**
            _connection.Closed += async (error) =>
            {
                Console.WriteLine($"[SignalR] Connection closed: {error?.Message}");
                // eventueel: try reconnect
                await Task.Delay(2000);
                await StartConnectionAsync();
            };
            _connection.Reconnecting += (error) =>
            {
                Console.WriteLine($"[SignalR] Reconnecting due to: {error?.Message}");
                return Task.CompletedTask;
            };
            _connection.Reconnected += (connectionId) =>
            {
                Console.WriteLine($"[SignalR] Reconnected. New connectionId: {connectionId}");
                return Task.CompletedTask;
            };

           
                RegisterEvents();
                
            

            await _connection.StartAsync();
        }

        private void RegisterEvents()
        {
            if (_connection == null) return;

            _connection.On<List<OrderDTO.Index>>("ReceiveOrders", orders =>
            {
                OnOrdersReceived?.Invoke(orders);
            });

            _connection.On<OrderDTO.Created>("OrderPlaced", order =>
            {
                OnOrderPlaced?.Invoke(order);
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

        public async Task PlaceOrder(OrderDTO.Create orderDto)
        {
            await StartConnectionAsync();
            if (await CheckConnectionAsync()) ;
            {
                // return-value ophalen
                var created = await _connection!
                    .InvokeAsync<OrderDTO.Created>("PlaceOrder", orderDto);

                // event vuren zodat de pagina weet dat 'ie gelukt is
                OnOrderPlaced?.Invoke(created);
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
