using AurHER.Models;
using AurHER.DTOs.Payment;

namespace AurHER.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentInitResult> InitializePaymentAsync(int orderId);
        Task<PaymentCallbackResult> HandleCallbackAsync(string transactionReference);
        Task HandleWebhookAsync(string jsonBody, string signature);
       Task<Payment?> GetPaymentByOrderIdAsync(int orderId);
      Task ResetPaymentForRetryAsync(int orderId);
       
    }


}