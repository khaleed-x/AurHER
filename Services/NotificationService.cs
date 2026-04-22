using AurHER.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text.Json;
using System.Text;

namespace AurHER.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IConfiguration config,
            ILogger<NotificationService> logger)
        {
            _config = config;
            _logger = logger;
        }

        // ── Email ──
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(
                    _config["EmailSettings:SenderName"],
                    _config["EmailSettings:SenderEmail"]));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = body };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    _config["EmailSettings:SmtpHost"],
                    int.Parse(_config["EmailSettings:SmtpPort"]!),
                    SecureSocketOptions.StartTls);

                await smtp.AuthenticateAsync(
                    _config["EmailSettings:SenderEmail"],
                    _config["EmailSettings:Password"]);

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
            }
        }

        // ── SMS ──
        public async Task SendSmsAsync(string to, string message)
        {
            try
            {
                var payload = new
                {
                    api_key = _config["SmsSettings:ApiKey"],
                    to = to,
                    from = _config["SmsSettings:SenderId"],
                    sms = message,
                    type = "plain",
                    channel = "generic"
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                var response = await client.PostAsync(
                    _config["SmsSettings:BaseUrl"], content);

                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning("SMS failed to {To}: {Status}", to, response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {To}", to);
            }
        }

   
       public async Task NotifyNewOrderAsync(int orderId, string customerName, 
            decimal total, string confirmationCode, string customerEmail)
        {
            var adminEmail = _config["AdminContact:Email"]!;
            var adminPhone = _config["AdminContact:Phone"]!;

            var subject = $"🛍️ New Order #{confirmationCode} — AurHER";
            var body = EmailTemplates.NewOrder(customerName, total, confirmationCode);
            await SendEmailAsync(adminEmail, subject, body);

        
            var sms = $"AurHER: New order from {customerName}! Order #{confirmationCode} — ₦{total:N0}. Check your dashboard.";
            await SendSmsAsync(adminPhone, sms);

             var customerSubject = $"Order Confirmed! #{confirmationCode} — AurHER";
            var customerBody = EmailTemplates.OrderConfirmation(customerName, total, confirmationCode);
            await SendEmailAsync(customerEmail, customerSubject, customerBody);

        }

        // ── Low Stock Notification ──
        public async Task NotifyLowStockAsync(string productName, string sku, int quantity)
        {
            var adminEmail = _config["AdminContact:Email"]!;
            var subject = $"⚠️ Low Stock Alert — {productName}";
            var body = EmailTemplates.LowStock(productName, sku, quantity);
            await SendEmailAsync(adminEmail, subject, body);
        }

        // ── Out of Stock Notification ──
        public async Task NotifyOutOfStockAsync(string productName, string sku)
        {
            var adminEmail = _config["AdminContact:Email"]!;
            var adminPhone = _config["AdminContact:Phone"]!;

            var subject = $"🚨 Out of Stock — {productName}";
            var body = EmailTemplates.OutOfStock(productName, sku);
            await SendEmailAsync(adminEmail, subject, body);

            var sms = $"AurHER: {productName} (SKU: {sku}) is now OUT OF STOCK. Please restock immediately.";
            await SendSmsAsync(adminPhone, sms);
        }

        // ── Order Status Changed ──
        public async Task NotifyOrderStatusChangedAsync(string customerEmail, string customerName, string confirmationCode, string newStatus)
        {
            var subject = $"Your AurHER Order #{confirmationCode} has been {newStatus}";
            var body = EmailTemplates.OrderStatusChanged(customerName, confirmationCode, newStatus);
            await SendEmailAsync(customerEmail, subject, body);
        }
    }
}