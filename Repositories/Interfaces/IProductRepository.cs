using AurHER.Models;

namespace AurHER.Repositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllWithDetailsAsync();
        Task<Product?> GetProductWithDetailsAsync(int id);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> GetActiveProductsAsync();
    }
}