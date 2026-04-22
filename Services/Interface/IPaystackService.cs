using AurHER.DTOs.Paystack;

namespace AurHER.Services.Interfaces
{
    public interface IPaystackService
    {
        Task<InitializePaymentResponse?> InitializePaymentAsync(string email, decimal amount, string reference, string callbackUrl);
        Task<VerifyPaymentResponse?> VerifyPaymentAsync(string reference);
    }
}