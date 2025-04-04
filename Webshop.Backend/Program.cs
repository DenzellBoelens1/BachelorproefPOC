using Microsoft.EntityFrameworkCore;
using Webshop.Backend.Data;
using Webshop.Backend.GraphQL;
using Webshop.Backend.Hubs;
using Webshop.Backend.Middleware;
using HotChocolate.Data;

var builder = WebApplication.CreateBuilder(args);

// 🔹 CORS configureren
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5113") // jouw Blazor frontend
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddFiltering()
    .AddSorting()
    .AddProjections();

builder.Services.AddSignalR();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();
app.UseCors(); // ✅ Middleware activeren vóór endpoints
app.MapHub<ProductHub>("/productHub");
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/productHub"), appBuilder =>
{
    appBuilder.UseMiddleware<ProductWebSocketMiddleware>();
});

//app.UseMiddleware<ProductWebSocketMiddleware>();
app.MapControllers();
app.MapGraphQL();


app.Run();
