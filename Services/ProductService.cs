using AurHER.Data;
using AurHER.DTOs.Admin;
using AurHER.Models;
using AurHER.Repositories.Interfaces;
using AurHER.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AurHER.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly INotificationService _notificationService;

        public ProductService(
            IProductRepository productRepository,
            AppDbContext context,
            IWebHostEnvironment environment, INotificationService notificationService)
        {
            _productRepository = productRepository;
            _context = context;
            _environment = environment;
             _notificationService = notificationService;
        }


        public async Task<IEnumerable<ProductListDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllWithDetailsAsync();
            return products.Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.Category?.Name,
                IsActive = p.IsActive,
                VariantCount = p.Variants?.Count ?? 0,
                LowestPrice = p.Variants?.Any() == true ? p.Variants.Min(v => v.Price) : null,
                HighestPrice = p.Variants?.Any() == true ? p.Variants.Max(v => v.Price) : null,
                PrimaryImage = p.Images?.FirstOrDefault(i => i.Isprimary)?.ImageUrl,
                CreatedAt = p.CreatedAt
            });
        }

        public async Task<ProductDetailDto?> GetProductWithDetailsAsync(int id)
        {
            var product = await _productRepository.GetProductWithDetailsAsync(id);
            if (product == null) return null;

            return new ProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                Variants = product.Variants?.Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    Size = v.Size,
                    Color = v.Color,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    ReservedStock = v.ReservedStock,
                    SKU = v.SKU
                }).ToList() ?? new(),
                Images = product.Images?.Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.Isprimary
                }).ToList() ?? new()
            };
        }

        public async Task<UpdateProductDto?> GetProductForEditAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;

            return new UpdateProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                CategoryId = product.CategoryId,
                IsActive = product.IsActive
            };
        }

        public async Task<bool> CreateProductAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product);
            return true;
        }

        public async Task<bool> UpdateProductAsync(UpdateProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(dto.Id);
            if (product == null) return false;

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.CategoryId = dto.CategoryId;
            product.IsActive = dto.IsActive;

            await _productRepository.UpdateAsync(product);
            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetProductWithDetailsAsync(id);
            if (product == null) return false;

            // Delete image files from wwwroot
            if (product.Images != null)
            {
                foreach (var image in product.Images)
                {
                    DeleteImageFile(image.ImageUrl);
                }
            }

            await _productRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> ToggleProductStatusAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return false;

            product.IsActive = !product.IsActive;
            await _productRepository.UpdateAsync(product);
            return true;
        }

        // ── Variants ──

        public async Task<bool> AddVariantAsync(CreateVariantDto dto)
        {
            // Check duplicate SKU
            var skuExists = await _context.ProductVariants
                .AnyAsync(v => v.SKU == dto.SKU);
            if (skuExists) return false;

            var variant = new ProductVariant
            {
                ProductId = dto.ProductId,
                Size = dto.Size,
                Color = dto.Color,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                SKU = dto.SKU
            };

            await _context.ProductVariants.AddAsync(variant);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UpdateVariantDto?> GetVariantForEditAsync(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant == null) return null;

            return new UpdateVariantDto
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                Size = variant.Size,
                Color = variant.Color,
                Price = variant.Price,
                StockQuantity = variant.StockQuantity,
                SKU = variant.SKU
            };
        }

        public async Task<bool> UpdateVariantAsync(UpdateVariantDto dto)
        {
            var variant = await _context.ProductVariants.FindAsync(dto.Id);
            if (variant == null) return false;

            // Check duplicate SKU — ignore current variant
            var skuExists = await _context.ProductVariants
                .AnyAsync(v => v.SKU == dto.SKU && v.Id != dto.Id);
            if (skuExists) return false;

            variant.Size = dto.Size;
            variant.Color = dto.Color;
            variant.Price = dto.Price;
            variant.StockQuantity = dto.StockQuantity;
            variant.SKU = dto.SKU;

            _context.ProductVariants.Update(variant);
            await _context.SaveChangesAsync();

                       
            if (dto.StockQuantity == 0)
            {
                await _notificationService.NotifyOutOfStockAsync(
                    variant.SKU, variant.SKU);
            }
            else if (dto.StockQuantity <= 5)
            {
                await _notificationService.NotifyLowStockAsync(
                    variant.SKU, variant.SKU, dto.StockQuantity);
            }
            return true;
        }

        public async Task<bool> DeleteVariantAsync(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant == null) return false;

            _context.ProductVariants.Remove(variant);
            await _context.SaveChangesAsync();
            return true;
        }

        // ── Images ──

        public async Task<bool> AddImageAsync(int productId, IFormFile file, bool isPrimary)
        {
            if (file == null || file.Length == 0) return false;

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension)) return false;

            // Create folder: wwwroot/images/products/{productId}/
            var folderPath = Path.Combine(
                _environment.WebRootPath, "images", "products", productId.ToString());
            Directory.CreateDirectory(folderPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // If this is primary, unset other primary images
            if (isPrimary)
            {
                var existingImages = await _context.ProductImages
                    .Where(i => i.ProductId == productId)
                    .ToListAsync();

                foreach (var img in existingImages)
                {
                    img.Isprimary = false;
                }
            }

            // Store path in DB
            var imageUrl = $"/images/products/{productId}/{fileName}";
            var productImage = new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl,
                Isprimary = isPrimary
            };

            await _context.ProductImages.AddAsync(productImage);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteImageAsync(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image == null) return false;

            // Delete physical file
            DeleteImageFile(image.ImageUrl);

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetPrimaryImageAsync(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image == null) return false;

            // Unset all other primary images for this product
            var allImages = await _context.ProductImages
                .Where(i => i.ProductId == image.ProductId)
                .ToListAsync();

            foreach (var img in allImages)
            {
                img.Isprimary = false;
            }

            image.Isprimary = true;
            await _context.SaveChangesAsync();
            return true;
        }

        // ── Private Helper ──

        private void DeleteImageFile(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var fullPath = Path.Combine(_environment.WebRootPath,
                imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}



