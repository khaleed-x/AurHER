using AurHER.DTOs.Store;

namespace AurHER.Services.Interfaces
{
    public interface ICheckoutService
    {
     Task<(int? OrderId, string ErrorMessage)> PlaceOrderAsync(string sessionId, CheckoutDto dto);
    }
}