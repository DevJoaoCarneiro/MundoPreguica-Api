using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.Interfaces;
using Application.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.ExternalServices
{
    public class ResendOrderEmailSender : IOrderEmailSender
    {
        private const string ResendEndpoint = "https://api.resend.com/emails";

        private readonly EmailNotificationSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ResendOrderEmailSender> _logger;

        public ResendOrderEmailSender(
            HttpClient httpClient,
            IOptions<EmailNotificationSettings> settings,
            ILogger<ResendOrderEmailSender> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendNewOrderAsync(OrderCreatedEmailNotification notification, CancellationToken cancellationToken = default)
        {
            if (!_settings.Enabled)
            {
                _logger.LogInformation("Envio de e-mail de pedido está desabilitado por configuração.");
                return;
            }

            var apiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = _settings.ResendApiKey;
            }

            var destination = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            if (string.IsNullOrWhiteSpace(destination))
            {
                destination = _settings.AdminEmail;
            }

            var from = _settings.From;
            if (string.IsNullOrWhiteSpace(from))
            {
                from = "MundoPreguica <onboarding@resend.dev>";
            }

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(destination))
            {
                _logger.LogWarning("Configuração de envio via Resend incompleta. Verifique RESEND_API_KEY/ADMIN_EMAIL ou seção EmailNotification.");
                return;
            }

            var recipients = destination
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (recipients.Length == 0)
            {
                _logger.LogWarning("Nenhum destinatário válido configurado para envio via Resend.");
                return;
            }

            var payload = new
            {
                from,
                to = recipients,
                subject = $"Novo pedido recebido - {notification.OrderId}",
                html = BuildBody(notification)
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, ResendEndpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Falha ao enviar e-mail via Resend. Status: {StatusCode}. Resposta: {Response}", response.StatusCode, errorContent);
                response.EnsureSuccessStatusCode();
            }
        }

        private static string BuildBody(OrderCreatedEmailNotification notification)
        {
            var body = new StringBuilder();
            body.AppendLine("<div style=\"font-family: Arial, sans-serif; color: #222;\">");
            body.AppendLine("  <h2 style=\"margin: 0 0 12px;\">Novo pedido recebido</h2>");
            body.AppendLine("  <p style=\"margin: 0 0 12px;\">Um novo pedido foi recebido. Seguem os detalhes:</p>");
            body.AppendLine("  <table style=\"border-collapse: collapse; width: 100%; margin-bottom: 16px;\">");
            body.AppendLine("    <tr><td style=\"padding: 6px 0; font-weight: bold; width: 140px;\">Pedido</td><td style=\"padding: 6px 0;\">" + notification.OrderId + "</td></tr>");
            body.AppendLine("    <tr><td style=\"padding: 6px 0; font-weight: bold;\">Data (UTC)</td><td style=\"padding: 6px 0;\">" + notification.OrderDate.ToString("yyyy-MM-dd HH:mm:ss") + "</td></tr>");
            body.AppendLine("    <tr><td style=\"padding: 6px 0; font-weight: bold;\">Tipo</td><td style=\"padding: 6px 0;\">" + notification.OrderType + "</td></tr>");
            body.AppendLine("    <tr><td style=\"padding: 6px 0; font-weight: bold;\">Cliente</td><td style=\"padding: 6px 0;\">" + notification.ClientName + "</td></tr>");
            body.AppendLine("    <tr><td style=\"padding: 6px 0; font-weight: bold;\">Telefone</td><td style=\"padding: 6px 0;\">" + notification.ClientPhone + "</td></tr>");
            body.AppendLine("    <tr><td style=\"padding: 6px 0; font-weight: bold;\">Total</td><td style=\"padding: 6px 0;\">R$ " + notification.TotalValue.ToString("F2") + "</td></tr>");
            body.AppendLine("  </table>");
            body.AppendLine("  <h3 style=\"margin: 0 0 8px;\">Itens</h3>");
            body.AppendLine("  <table style=\"border-collapse: collapse; width: 100%;\">");
            body.AppendLine("    <thead>");
            body.AppendLine("      <tr>");
            body.AppendLine("        <th style=\"text-align: left; border-bottom: 1px solid #ddd; padding: 8px 4px;\">Produto</th>");
            body.AppendLine("        <th style=\"text-align: left; border-bottom: 1px solid #ddd; padding: 8px 4px;\">Tamanho</th>");
            body.AppendLine("        <th style=\"text-align: left; border-bottom: 1px solid #ddd; padding: 8px 4px;\">Quantidade</th>");
            body.AppendLine("        <th style=\"text-align: left; border-bottom: 1px solid #ddd; padding: 8px 4px;\">Valor unitario</th>");
            body.AppendLine("      </tr>");
            body.AppendLine("    </thead>");
            body.AppendLine("    <tbody>");

            foreach (var item in notification.Items)
            {
                body.AppendLine("      <tr>");
                body.AppendLine("        <td style=\"padding: 8px 4px; border-bottom: 1px solid #f0f0f0;\">" + item.ProductName + "</td>");
                body.AppendLine("        <td style=\"padding: 8px 4px; border-bottom: 1px solid #f0f0f0;\">" + item.Size + "</td>");
                body.AppendLine("        <td style=\"padding: 8px 4px; border-bottom: 1px solid #f0f0f0;\">" + item.Quantity + "</td>");
                body.AppendLine("        <td style=\"padding: 8px 4px; border-bottom: 1px solid #f0f0f0;\">R$ " + item.UnitPrice.ToString("F2") + "</td>");
                body.AppendLine("      </tr>");
            }

            body.AppendLine("    </tbody>");
            body.AppendLine("  </table>");
            body.AppendLine("</div>");

            return body.ToString();
        }
    }
}