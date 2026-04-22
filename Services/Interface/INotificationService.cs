namespace AurHER.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendSmsAsync(string to, string message);
        Task NotifyNewOrderAsync(int orderId, string customerName, decimal total, 
            string confirmationCode, string customerEmail);
        Task NotifyLowStockAsync(string productName, string sku, int quantity);
        Task NotifyOutOfStockAsync(string productName, string sku);
        Task NotifyOrderStatusChangedAsync(string customerEmail, string customerName, string confirmationCode, string newStatus);
    }
}