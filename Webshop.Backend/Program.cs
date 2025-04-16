using Microsoft.EntityFrameworkCore;
using Webshop.Backend.Data;
using Webshop.Backend.GraphQL;
using Webshop.Backend.Hubs;

using HotChocolate.Data;
using Webshop.Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Webshop.Backend.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 🔹 CORS configureren
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5113")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ThisIsASuperSecureJwtKeyThatIsAtLeast32BytesLong!")) //AANPASSEN
    };
});

//services
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();


builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
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

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();
app.UseCors(); // ✅ Middleware activeren vóór endpoints

// SignalR
app.MapHub<ProductHub>("/signalr/product");
app.MapHub<OrderHub>("/signalr/order");

// WebSocket middleware
app.UseWhen(context => context.Request.Path.StartsWithSegments("/ws/product"), appBuilder =>
{
    appBuilder.UseMiddleware<ProductWebSocketMiddleware>();
});
app.UseWhen(context => context.Request.Path.StartsWithSegments("/ws/order"), appBuilder =>
{
    appBuilder.UseMiddleware<OrderWebSocketMiddleware>();
});

app.MapControllers();
app.MapGraphQL();



app.Run();
