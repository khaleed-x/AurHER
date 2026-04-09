using System.Security.Cryptography.X509Certificates;
using AurHER.Data;
using AurHER.DTOs.Admin;
using AurHER.Models;
using AurHER.Models.Enums;
using AurHER.Repositories.Interfaces;
using AurHER.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;


namespace AurHER.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IOrderRepository _orderRepository;
        private readonly INotificationService _notificationService;

        public OrderService(AppDbContext context, IOrderRepository orderRepository, INotificationService notificationService)
        {
            _context = context;
            _orderRepository = orderRepository;
            _notificationService = notificationService;
        }


        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _orderRepository.GetByIdAsync(id);
        }

        // Complex query — use _context directly
  public async Task<OrderDashboardDto> GetAllOrdersAsync(
    string? search = null, 
    OrderStatus? status = null,
    PaymentStatus? paymentStatus = null)
{
    var query = _context.Orders
        .Include(o => o.Payment)
        .AsQueryable();
    
    var today = DateTime.UtcNow.Date;
    var totalOrderCount = await _context.Orders.CountAsync();
    var todayOrder = await _context.Orders.CountAsync(o => o.CreatedAt.Date == today);

    // Apply search
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(o => 
            o.ConfirmationCode.Contains(search) ||
            o.CustomerName.Contains(search) ||
            o.Email.Contains(search));
    }

    // Apply order status filter
    if (status.HasValue)
    {
        query = query.Where(o => o.Status == status.Value);
    }

    // Apply payment status filter
    if (paymentStatus.HasValue)
    {
        query = query.Where(o => o.Payment != null && o.Payment.Status == paymentStatus.Value);
    }

    var orders = await query
        .OrderByDescending(o => o.CreatedAt)
        .Select(o => new OrderListDto
        {
            Id = o.Id,
            ConfirmationCode = o.ConfirmationCode,
            CustomerName = o.CustomerName,
            Email = o.Email,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            PaymentStatus = o.Payment != null ? o.Payment.Status : PaymentStatus.Pending,
            CreatedAt = o.CreatedAt
        })
        .ToListAsync();

    return new OrderDashboardDto
    {
        Counts = new CountDto
        {
            TotalOrderCount = totalOrderCount,
            OrderToday = todayOrder
        },
        Orders = orders
    };
}

        // Complex query with nested includes — use _context directly
        public async Task<OrderDetailDto?> GetOrderWithDetailsAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(v => v.Product)
                            .ThenInclude(p => p.Images)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;

            return new OrderDetailDto
            {
                Id = order.Id,
                ConfirmationCode = order.ConfirmationCode,
                CustomerName = order.CustomerName,
                Email = order.Email,
                Phone = order.Phone,
                ShippingAddress = order.ShippingAddress,
                DeliveryFee = order.DeliveryFee,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                Payment = order.Payment != null ? new PaymentInfoDto
                {
                    Status = order.Payment.Status,
                    Amount = order.Payment.Amount,
                    PaymentMethod = order.Payment.PaymentMethod,
                    TransactionReference = order.Payment.TransactionReference,
                    PaidAt = order.Payment.PaidAt
                } : null,
                Items = order.Items.Select(i => new OrderItemDetailDto
                {
                    ProductName = i.ProductVariant?.Product?.Name ?? "Unknown",
                    Variant = $"{i.ProductVariant?.Size} / {i.ProductVariant?.Color}",
                    Quantity = i.Quantity,
                    Price = i.Price,
                    Subtotal = i.Price * i.Quantity,
                    ProductImage = i.ProductVariant?.Product?.Images?
                        .FirstOrDefault(img => img.Isprimary)?.ImageUrl
                }).ToList()
            };
        }

       
        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return false;

            // Prevent invalid transitions
            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
                return false;

            order.Status = newStatus;
            await _orderRepository.UpdateAsync(order);
          
            await _notificationService.NotifyOrderStatusChangedAsync(
                order.Email,
                order.CustomerName,
                order.ConfirmationCode,
                newStatus.ToString());
            return true;
        }
    }
}