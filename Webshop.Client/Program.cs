using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Webshop.Client;
using Webshop.Client.Auth;
using Webshop.Client.Layout;
using Webshop.Client.Services.GraphQL;
using Webshop.Client.Services.REST;
using Webshop.Client.Services.SignalR;
using Webshop.Client.Services.Websockets;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5139/")
});

builder.Services.AddScoped<AppState>();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddScoped<JwtAuthStateProvider>(); // Eigen implementatie
builder.Services.AddScoped<TokenService>(); // Voor tokenbeheer

builder.Services.AddScoped<ProductRestService>();
builder.Services.AddScoped<OrderRestService>();

builder.Services.AddScoped<ProductGraphQLService>();
builder.Services.AddScoped<OrderGraphQLService>();

builder.Services.AddScoped<ProductSignalRService>();
builder.Services.AddScoped<OrderSignalRService>();

builder.Services.AddScoped<ProductWebSocketService>();
builder.Services.AddScoped<OrderWebSocketService>();

await builder.Build().RunAsync();
