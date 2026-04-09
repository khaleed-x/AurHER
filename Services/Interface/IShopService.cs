using AurHER.DTOs.Store;
using AurHER.DTOs.Admin;

namespace AurHER.Services.Interfaces
{
    public interface IShopService
    {
        Task<HomePageDto> GetHomePageDataAsync();
        Task<IEnumerable<HomeProductDto>> GetAllProductsAsync(string? category = null, string? search = null, string? sort = null);
        Task<HomeProductDetailDto?> GetProductDetailAsync(int id);
    }
}