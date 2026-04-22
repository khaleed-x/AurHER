using AurHER.Data;
using AurHER.Models.Enums;
using AurHER.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AurHER.Services
{
    public class CleanupService : ICleanupService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CleanupService> _logger;

        public CleanupService(AppDbContext context, ILogger<CleanupService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> CleanupStaleOrdersAsync(int timeoutMinutes = 20)
        {
            var staleTime = DateTime.UtcNow.AddMinutes(-timeoutMinutes);
            
            // 1. Find stale pending orders
            var staleOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Pending && o.CreatedAt < staleTime)
                .Include(o => o.Items)
                .Include(o => o.Payment)
                .ToListAsync();

            if (!staleOrders.Any())
            {
                _logger.LogInformation("No stale orders found");
                return 0;
            }

            foreach (var order in staleOrders)
            {
                // 2. Release reserved variants for this order
                foreach (var item in order.Items)
                {
                    var variant = await _context.ProductVariants.FindAsync(item.ProductVariantId);
                    if (variant != null)
                    {
                        
                        variant.ReservedStock -= item.Quantity;
                        _logger.LogDebug($"Released reservation for variant {item.ProductVariantId}, quantity {item.Quantity}");
                    }
                }
                
                // 3. Cancel the order
                order.Status = OrderStatus.Cancelled;
                
                // 4. Mark payment as failed
                if (order.Payment != null)
                {
                    order.Payment.Status = PaymentStatus.Failed;
                }
                
                _logger.LogDebug($"Cleaned up order {order.Id} ({order.ConfirmationCode})");
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Cleaned up {staleOrders.Count} stale orders (reservations released, orders cancelled, payments failed)");
            
            return staleOrders.Count;
        }
    }
}