using AurHER.DTOs.Admin;
using Microsoft.AspNetCore.Http;

namespace AurHER.Services.Interfaces
{
    public interface IProductService
    {
        // Products
        Task<IEnumerable<ProductListDto>> GetAllProductsAsync();
        Task<ProductDetailDto?> GetProductWithDetailsAsync(int id);
        Task<UpdateProductDto?> GetProductForEditAsync(int id);
        Task<bool> CreateProductAsync(CreateProductDto dto);
        Task<bool> UpdateProductAsync(UpdateProductDto dto);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> ToggleProductStatusAsync(int id);

        // Variants
        Task<bool> AddVariantAsync(CreateVariantDto dto);
        Task<UpdateVariantDto?> GetVariantForEditAsync(int id);
        Task<bool> UpdateVariantAsync(UpdateVariantDto dto);
        Task<bool> DeleteVariantAsync(int id);

        // Images
        Task<bool> AddImageAsync(int productId, IFormFile file, bool isPrimary);
        Task<bool> DeleteImageAsync(int id);
        Task<bool> SetPrimaryImageAsync(int id);
    }
}