using AurHER.DTOs.Admin;
using AurHER.Models.Enums;
using AurHER.Models;

namespace AurHER.Services.Interfaces
{
    public interface IOrderService
    {
       Task<Order?> GetByIdAsync(int id);
       Task<OrderDashboardDto> GetAllOrdersAsync( 
         string? search = null,  
         OrderStatus? status = null, PaymentStatus? paymentStatus = null);
        Task<OrderDetailDto?> GetOrderWithDetailsAsync(int id);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
    }
}