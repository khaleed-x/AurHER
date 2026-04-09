using AurHER.DTOs.Admin;

namespace AurHER.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryItemDto>> GetInventoryAsync();
        Task<IEnumerable<InventoryItemDto>> GetLowStockAsync();
        Task<IEnumerable<InventoryItemDto>> GetOutOfStockAsync();
    }
}