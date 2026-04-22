using AurHER.DTOs.Store;

namespace AurHER.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(string sessionId);
        Task<int> GetCartCountAsync(string sessionId);
         Task<AddToCartResult> AddToCartAsync(string sessionId, AddToCartDto dto);
        Task UpdateQuantityAsync(string sessionId, int cartItemId, int quantity);
        Task RemoveFromCartAsync(string sessionId, int cartItemId);
        Task ClearCartAsync(string sessionId);
    }
}