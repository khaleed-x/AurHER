using AurHER.Models;

namespace AurHER.Repositories.Interfaces
{
    public interface IDeliveryLocationRepository : IRepository<DeliveryLocation>
    {
        Task<IEnumerable<DeliveryLocation>> GetAllActiveAsync();
    }
}