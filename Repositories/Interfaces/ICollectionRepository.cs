using AurHER.Models;

namespace AurHER.Repositories.Interfaces
{
    public interface ICollectionRepository : IRepository<Collection>
    {
        Task<IEnumerable<Collection>> GetAllWithProductsAsync();
        Task<Collection?> GetCollectionWithProductsAsync(int id);
        Task AddProductToCollectionAsync(int collectionId, int productId);
        Task RemoveProductFromCollectionAsync(int collectionId, int productId);
        Task<bool> ProductExistsInCollectionAsync(int collectionId, int productId);
    }
}