using AurHER.Models;
using AurHER.Models.Enums;

namespace AurHER.Repositories.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetOrderWithDetailsAsync(int id);
        Task<Order?> GetOrderByConfirmationCodeAsync(string confirmationCode);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
    }
}

