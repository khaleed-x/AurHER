using AurHER.DTOs.Admin;
using AurHER.DTOs.Store;

namespace AurHER.Services.Interfaces
{
    public interface ICollectionService
    {
        Task<IEnumerable<CollectionDto>> GetAllCollectionsAsync();
        Task<CollectionDto?> GetCollectionByIdAsync(int id);
        Task<CollectionDetailDto?> GetCollectionWithProductsAsync(int id);
        Task<bool> CreateCollectionAsync(CreateCollectionDto dto);
        Task<bool> UpdateCollectionAsync(UpdateCollectionDto dto);
        Task<bool> DeleteCollectionAsync(int id);
        Task AddProductToCollectionAsync(AddProductToCollectionDto dto);
        Task RemoveProductFromCollectionAsync(int collectionId, int productId);
        Task<bool> CollectionExistsAsync(string name);
        Task<List<HomeCollectionDto>> GetPublicCollectionsAsync();
    }
}