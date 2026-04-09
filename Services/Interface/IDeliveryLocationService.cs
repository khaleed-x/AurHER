using AurHER.Models;

namespace AurHER.Services.Interfaces
{
    public interface IDeliveryLocationService
    {
        Task<IEnumerable<DeliveryLocation>> GetAllLocationsAsync();
        Task<DeliveryLocation?> GetLocationByIdAsync(int id);
        Task<bool> CreateLocationAsync(DeliveryLocation location);
        Task<bool> UpdateLocationAsync(DeliveryLocation location);
        Task<bool> DeleteLocationAsync(int id);
    }
}