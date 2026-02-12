using Application.Interfaces;
using Application.Service;
using Application.Services;
using Domain.Events;
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
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddScoped<IDomainEventHandler<OrderCreatedEvent>, OrderCreatedEventHandler>();
builder.Services.AddSingleton<IOrderCreatedEmailQueue, OrderCreatedEmailQueue>();
builder.Services.AddScoped<IOrderEmailSender, SmtpOrderEmailSender>();
builder.Services.AddHostedService<OrderCreatedEmailHostedService>();
builder.Services.Configure<EmailNotificationSettings>(builder.Configuration.GetSection("EmailNotification"));


var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(rawConnectionString))
{
    throw new Exception("Connection string 'DefaultConnection' nao encontrada.");
}

string connectionString;

if (Uri.TryCreate(rawConnectionString, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.Host))
{
    var userInfo = uri.UserInfo.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
    var username = userInfo.Length > 0 ? userInfo[0] : string.Empty;
    var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
    var database = uri.AbsolutePath.TrimStart('/');

    connectionString =
        $"Host={uri.Host};" +
        $"Port={uri.Port};" +
        $"Database={database};" +
        $"Username={username};" +
        $"Password={password};" +
        $"SSL Mode=Require;Trust Server Certificate=true";
}
else
{
    connectionString = rawConnectionString;
}

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

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();

