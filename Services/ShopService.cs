using AurHER.Data;
using AurHER.DTOs.Admin;
using AurHER.DTOs.Store;
using AurHER.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AurHER.Services
{
    public class ShopService : IShopService
    {
        private readonly AppDbContext _context;

        public ShopService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<HomePageDto> GetHomePageDataAsync()
        {
            // Featured categories — only those with active products
            var categories = await _context.Categories
                .Include(c => c.Products)
                .Where(c => c.Products.Any(p => p.IsActive))
                .Select(c => new HomeCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products.Count(p => p.IsActive)
                })
                .ToListAsync();

            // Collections with their first 4 active products
            var collections = await _context.Collections
                .Include(c => c.Products)
                    .ThenInclude(cp => cp.Product)
                        .ThenInclude(p => p.Images)
                .Include(c => c.Products)
                    .ThenInclude(cp => cp.Product)
                        .ThenInclude(p => p.Variants)
                .Where(c => c.Products.Any(cp => cp.Product.IsActive))
                .Select(c => new HomeCollectionDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Products = c.Products
                        .Where(cp => cp.Product.IsActive)
                        .Take(4)
                        .Select(cp => new HomeProductDto
                        {
                            Id = cp.Product.Id,
                            Name = cp.Product.Name,
                            PrimaryImage = cp.Product.Images
                                .FirstOrDefault(i => i.Isprimary) != null
                                ? cp.Product.Images.FirstOrDefault(i => i.Isprimary)!.ImageUrl
                                : cp.Product.Images.FirstOrDefault() != null
                                ? cp.Product.Images.FirstOrDefault()!.ImageUrl
                                : null,
                            LowestPrice = cp.Product.Variants.Any()
                                ? cp.Product.Variants.Min(v => v.Price)
                                : null,
                            HighestPrice = cp.Product.Variants.Any()
                                ? cp.Product.Variants.Max(v => v.Price)
                                : null,
                            HasVariants = cp.Product.Variants.Any()
                        }).ToList()
                })
                .ToListAsync();

            // New arrivals — latest 8 active products
            var newArrivals = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .Select(p => new HomeProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    PrimaryImage = p.Images.FirstOrDefault(i => i.Isprimary) != null
                        ? p.Images.FirstOrDefault(i => i.Isprimary)!.ImageUrl
                        : p.Images.FirstOrDefault() != null
                        ? p.Images.FirstOrDefault()!.ImageUrl
                        : null,
                    LowestPrice = p.Variants.Any() ? p.Variants.Min(v => v.Price) : null,
                    HighestPrice = p.Variants.Any() ? p.Variants.Max(v => v.Price) : null,
                    HasVariants = p.Variants.Any()
                })
                .ToListAsync();

            return new HomePageDto
            {
                Categories = categories,
                Collections = collections,
                NewArrivals = newArrivals
            };
        }

        public async Task<IEnumerable<HomeProductDto>> GetAllProductsAsync(
            string? category = null,
            string? search = null,
            string? sort = null)
        {
            var query = _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
                query = query.Where(p => p.Category != null &&
                    p.Category.Name.ToLower() == category.ToLower());

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p =>
                    p.Name.ToLower().Contains(search.ToLower()) ||
                    p.Description.ToLower().Contains(search.ToLower()));

            query = sort switch
            {
                "price-asc" => query.OrderBy(p => p.Variants.Min(v => v.Price)),
                "price-desc" => query.OrderByDescending(p => p.Variants.Max(v => v.Price)),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            return await query.Select(p => new HomeProductDto
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.Category != null ? p.Category.Name : null,
                PrimaryImage = p.Images.FirstOrDefault(i => i.Isprimary) != null
                    ? p.Images.FirstOrDefault(i => i.Isprimary)!.ImageUrl
                    : p.Images.FirstOrDefault() != null
                    ? p.Images.FirstOrDefault()!.ImageUrl
                    : null,
                LowestPrice = p.Variants.Any() ? p.Variants.Min(v => v.Price) : null,
                HighestPrice = p.Variants.Any() ? p.Variants.Max(v => v.Price) : null,
                HasVariants = p.Variants.Any()
            }).ToListAsync();
        }

        public async Task<HomeProductDetailDto?> GetProductDetailAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null) return null;

            return new HomeProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                CategoryName = product.Category?.Name,
                Images = product.Images.Select(i => new HomeImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.Isprimary
                }).ToList(),
                Variants = product.Variants.Select(v => new HomeVariantDto
                {
                    Id = v.Id,
                    Size = v.Size,
                    Color = v.Color,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    SKU = v.SKU
                }).ToList()
            };
        }
    }
}