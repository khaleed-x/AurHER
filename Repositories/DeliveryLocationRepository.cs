using AurHER.Data;
using AurHER.Models;
using AurHER.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AurHER.Repositories
{
    public class DeliveryLocationRepository : Repository<DeliveryLocation>, IDeliveryLocationRepository
    {
        public DeliveryLocationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DeliveryLocation>> GetAllActiveAsync()
        {
            return await _context.DeliveryLocations
                .OrderBy(l => l.Name)
                .ToListAsync();
        }
    }
}