using Microsoft.EntityFrameworkCore;
using AurHER.Data;
using AurHER.Models;
using AurHER.Models.Enums;
using AurHER.Repositories.Interfaces;

namespace AurHER.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders.ToListAsync();
        }

    
        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders.FindAsync(id);
        }

        // Get order with all its details
        public async Task<Order?> GetOrderWithDetailsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(v => v.Product)
                            .ThenInclude(p => p.Images)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        // Get order by confirmation code (for guest order tracking)
        public async Task<Order?> GetOrderByConfirmationCodeAsync(string confirmationCode)
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(v => v.Product)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.ConfirmationCode == confirmationCode);
        }

        // Get orders filtered by status (e.g. all Pending orders)
        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .Where(o => o.Status == status)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // Update only the status of an order
        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await GetByIdAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await GetByIdAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }
    }
}