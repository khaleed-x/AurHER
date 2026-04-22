using AurHER.Data;
using AurHER.DTOs.Admin;
using AurHER.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AurHER.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly AppDbContext _context;

        public InventoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InventoryItemDto>> GetInventoryAsync()
        {
            return await _context.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p.Category)
                .OrderBy(v => v.Product.Name)
                .Select(v => new InventoryItemDto
                {
                    VariantId = v.Id,
                    ProductId = v.ProductId,
                    ProductName = v.Product.Name,
                    SKU = v.SKU,
                    Size = v.Size,
                    Color = v.Color,
                    StockQuantity = v.StockQuantity,
                    CategoryName = v.Product.Category != null ? v.Product.Category.Name : null,
                    StockStatus = v.StockQuantity == 0 ? "Out of Stock"
                                : v.StockQuantity <= 5 ? "Low Stock"
                                : "In Stock"
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryItemDto>> GetLowStockAsync()
        {
            return await _context.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p.Category)
                .Where(v => v.StockQuantity > 0 && v.StockQuantity <= 5)
                .OrderBy(v => v.StockQuantity)
                .Select(v => new InventoryItemDto
                {
                    VariantId = v.Id,
                    ProductId = v.ProductId,
                    ProductName = v.Product.Name,
                    SKU = v.SKU,
                    Size = v.Size,
                    Color = v.Color,
                    StockQuantity = v.StockQuantity,
                    CategoryName = v.Product.Category != null ? v.Product.Category.Name : null,
                    StockStatus = "Low Stock"
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryItemDto>> GetOutOfStockAsync()
        {
            return await _context.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p.Category)
                .Where(v => v.StockQuantity == 0)
                .OrderBy(v => v.Product.Name)
                .Select(v => new InventoryItemDto
                {
                    VariantId = v.Id,
                    ProductId = v.ProductId,
                    ProductName = v.Product.Name,
                    SKU = v.SKU,
                    Size = v.Size,
                    Color = v.Color,
                    StockQuantity = v.StockQuantity,
                    CategoryName = v.Product.Category != null ? v.Product.Category.Name : null,
                    StockStatus = "Out of Stock"
                })
                .ToListAsync();
        }
    }
}