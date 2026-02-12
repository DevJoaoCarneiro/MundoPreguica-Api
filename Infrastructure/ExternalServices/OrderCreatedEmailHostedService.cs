using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ExternalServices
{
    public class OrderCreatedEmailHostedService : BackgroundService
    {
        private readonly IOrderCreatedEmailQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrderCreatedEmailHostedService> _logger;

        public OrderCreatedEmailHostedService(
            IOrderCreatedEmailQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<OrderCreatedEmailHostedService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var notification = await _queue.DequeueAsync(stoppingToken);

                    using var scope = _scopeFactory.CreateScope();
                    var sender = scope.ServiceProvider.GetRequiredService<IOrderEmailSender>();

                    await sender.SendNewOrderAsync(notification, stoppingToken);

                    _logger.LogInformation("E-mail de novo pedido enviado para OrderId: {OrderId}", notification.OrderId);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("HostedService de envio de e-mail finalizado.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no processamento assíncrono de e-mail de pedido.");
                    await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                }
            }
        }
    }
}
