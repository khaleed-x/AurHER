using AurHER.Models;

namespace AurHER.Repositories.Interfaces
{
    public interface ICartRepository : IRepository<Cart>
    {
        Task<Cart?> GetCartBySessionAsync(string sessionId);
        Task<Cart?> GetCartWithItemsAsync(string sessionId);
        Task AddItemToCartAsync(int cartId, int productVariantId, int quantity);
        Task RemoveItemFromCartAsync(int cartItemId);
        Task UpdateItemQuantityAsync(int cartItemId, int quantity);
        Task ClearCartAsync(int cartId);
        Task<CartItem?> GetCartItemAsync(int cartId, int productVariantId);
    }
}