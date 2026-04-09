using AurHER.Models;
using AurHER.Repositories.Interfaces;
using AurHER.Services.Interfaces;

namespace AurHER.Services
{
    public class DeliveryLocationService : IDeliveryLocationService
    {
        private readonly IDeliveryLocationRepository _repository;

        public DeliveryLocationService(IDeliveryLocationRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DeliveryLocation>> GetAllLocationsAsync()
        {
            return await _repository.GetAllActiveAsync();
        }

        public async Task<DeliveryLocation?> GetLocationByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> CreateLocationAsync(DeliveryLocation location)
        {
            await _repository.AddAsync(location);
            return true;
        }

        public async Task<bool> UpdateLocationAsync(DeliveryLocation location)
        {
            await _repository.UpdateAsync(location);
            return true;
        }

        public async Task<bool> DeleteLocationAsync(int id)
        {
            await _repository.DeleteAsync(id);
            return true;
        }
    }
}