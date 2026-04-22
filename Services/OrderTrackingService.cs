using AurHER.Data;
using AurHER.DTOs.Store;
using AurHER.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AurHER.Services
{
    public class OrderTrackingService : IOrderTrackingService
    {
        private readonly AppDbContext _context;

        public OrderTrackingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderTrackingDto?> GetOrderByCodeAsync(string confirmationCode)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(v => v.Product)
                            .ThenInclude(p => p.Images)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.ConfirmationCode == confirmationCode.ToUpper().Trim());

            if (order == null) return null;

            return new OrderTrackingDto
            {
                ConfirmationCode = order.ConfirmationCode,
                CustomerName = order.CustomerName,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                PaymentStatus = order.Payment?.Status ?? AurHER.Models.Enums.PaymentStatus.Pending,
                Items = order.Items.Select(i => new OrderTrackingItemDto
                {
                    ProductName = i.ProductVariant?.Product?.Name ?? "",
                    Size = i.ProductVariant?.Size ?? "",
                    Color = i.ProductVariant?.Color ?? "",
                    Quantity = i.Quantity,
                    Price = i.Price,
                    PrimaryImage = i.ProductVariant?.Product?.Images?
                        .FirstOrDefault(img => img.Isprimary)?.ImageUrl
                        ?? i.ProductVariant?.Product?.Images?.FirstOrDefault()?.ImageUrl
                }).ToList()
            };
        }
    }
}