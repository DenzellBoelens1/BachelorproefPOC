using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Webshop.Client;
using Webshop.Client.Layout;
using Webshop.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5139/")
});

builder.Services.AddScoped<AppState>();

builder.Services.AddScoped<ProductRestService>();
builder.Services.AddScoped<ProductGraphQLService>();
builder.Services.AddScoped<ProductSignalRService>();
builder.Services.AddScoped<ProductWebSocketService>();

await builder.Build().RunAsync();
