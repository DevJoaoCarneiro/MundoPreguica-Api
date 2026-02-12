using Application.Interfaces;
using Application.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Infrastructure.ExternalServices
{
    public class SmtpOrderEmailSender : IOrderEmailSender
    {
        private readonly EmailNotificationSettings _settings;
        private readonly ILogger<SmtpOrderEmailSender> _logger;

        public SmtpOrderEmailSender(IOptions<EmailNotificationSettings> settings, ILogger<SmtpOrderEmailSender> logger)
        {
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

            if (string.IsNullOrWhiteSpace(_settings.Host) ||
                string.IsNullOrWhiteSpace(_settings.From) ||
                string.IsNullOrWhiteSpace(_settings.To))
            {
                _logger.LogWarning("Configuração de e-mail incompleta. Verifique seção EmailNotification.");
                return;
            }

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.UseSsl,
                Credentials = string.IsNullOrWhiteSpace(_settings.Username)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(_settings.Username, _settings.Password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.From),
                Subject = $"Novo pedido recebido - {notification.OrderId}",
                Body = BuildBody(notification),
                IsBodyHtml = true
            };

            foreach (var address in _settings.To.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                message.To.Add(address);
            }

            await client.SendMailAsync(message, cancellationToken);
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
