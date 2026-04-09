using AurHER.DTOs.Admin;
using AurHER.DTOs.Store;

namespace AurHER.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<bool> CreateCategoryAsync(CreateCategoryDto dto);
        Task<bool> UpdateCategoryAsync(UpdateCategoryDto dto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> CategoryExistsAsync(string name);
        Task<List<HomeCategoryDto>> GetPublicCategoriesAsync();
    }
}