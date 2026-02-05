using Application.Interfaces;
using Application.Service;
using Application.Services;
using Domain.Interfaces;
using Domain.Repository;
using Infrastructure.Context;
using Infrastructure.ExternalServices;
using Infrastructure.Persistence;
using Infrastructure.Provider;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUserServices, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISecurityService, BCryptoSecurityService>();
builder.Services.AddScoped<IProductServices, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IImageUploadService, CloudinaryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, JwtTokenProvider>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();


var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(rawConnectionString))
{
    throw new Exception("Connection string 'DefaultConnection' não encontrada.");
}

var uri = new Uri(rawConnectionString);
var userInfo = uri.UserInfo.Split(':');

var connectionString =
    $"Host={uri.Host};" +
    $"Port={uri.Port};" +
    $"Database={uri.AbsolutePath.TrimStart('/')};" +
    $"Username={userInfo[0]};" +
    $"Password={userInfo[1]};" +
    $"SSL Mode=Require;Trust Server Certificate=true";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        Console.WriteLine("Migrations aplicadas com sucesso.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao aplicar migrations: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();

