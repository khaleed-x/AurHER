using AurHER.Models;

namespace AurHER.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetCategoryWithProductsAsync(int id);
        Task<bool> CategoryExistsAsync(string name);
    }
}