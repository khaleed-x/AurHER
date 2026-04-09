using AurHER.DTOs.Store;

namespace AurHER.Services.Interfaces
{
    public interface IOrderTrackingService
    {
        Task<OrderTrackingDto?> GetOrderByCodeAsync(string confirmationCode);
    }
}