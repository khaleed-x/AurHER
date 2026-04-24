using AurHER.Data;
using AurHER.DTOs.Admin;
using AurHER.Models;
using AurHER.Repositories.Interfaces;
using AurHER.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;


namespace AurHER.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly INotificationService _notificationService;
        private readonly IImageCompressionService _imageCompressionService;
        private readonly Cloudinary _cloudinary;
        public ProductService(
            IProductRepository productRepository,
            AppDbContext context,
            IWebHostEnvironment environment, INotificationService notificationService, 
            IImageCompressionService imageCompressionService,
            Cloudinary cloudinary)
        {
            _productRepository = productRepository;
            _context = context;
            _environment = environment;
             _notificationService = notificationService;
             _imageCompressionService = imageCompressionService;
             _cloudinary = cloudinary;
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


                // Delete images from Cloudinary
                if (product.Images != null)
                {
                    foreach (var image in product.Images)
                    {
                        if (!string.IsNullOrEmpty(image.ImageUrl) && image.ImageUrl.Contains("cloudinary.com"))
                        {
                            try
                            {
                                var publicId = ExtractPublicIdFromUrl(image.ImageUrl);
                                if (!string.IsNullOrEmpty(publicId))
                                {
                                    await _cloudinary.DestroyAsync(new DeletionParams(publicId));
                                }
                            }
                            catch
                            {
                                // Log error but continue
                            }
                        }
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
            if (variant == null ) return false;

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

            if (file.Length > 5 * 1024 * 1024)
                return false;

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension)) return false;

              // Compress and resize image
            var compressedBytes = await _imageCompressionService.CompressAndResizeAsync(file, 800, 800, 75);
              
                if (compressedBytes == null)
                    return false;

            using var stream = new MemoryStream(compressedBytes);

        var fileName = $"{Guid.NewGuid()}.webp";

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = $"products/{productId}"
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.StatusCode != System.Net.HttpStatusCode.OK)
        {
           return false;
        }

        var imageUrl = result.SecureUrl.ToString();

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

            // Delete from Cloudinary
            if (!string.IsNullOrEmpty(image.ImageUrl) && image.ImageUrl.Contains("cloudinary.com"))
            {
                try
                {
                    var publicId = ExtractPublicIdFromUrl(image.ImageUrl);
                    if (!string.IsNullOrEmpty(publicId))
                    {
                        var deletionParams = new DeletionParams(publicId);
                        await _cloudinary.DestroyAsync(deletionParams);
                    }
                }
                catch
                {
                    // Log error but continue with database deletion
                }
            }

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

        private string ExtractPublicIdFromUrl(string imageUrl)
        {
            try
            {
                var uri = new Uri(imageUrl);
                var path = uri.AbsolutePath;

                // Find "/upload/" — everything useful is after this
                var uploadIndex = path.IndexOf("/upload/");
                if (uploadIndex < 0) return null;

                var afterUpload = path.Substring(uploadIndex + "/upload/".Length);
                // afterUpload = "v1234567890/products/123/abc123.webp"

                var segments = afterUpload.Split('/');

                // Skip version segment if it starts with 'v' followed by digits
                int startIndex = 0;
                if (segments[0].StartsWith("v") && long.TryParse(segments[0].Substring(1), out _))
                    startIndex = 1;

                var relevant = segments.Skip(startIndex).ToArray();
                // relevant = ["products", "123", "abc123.webp"]

                // Strip extension from last segment
                relevant[relevant.Length - 1] = Path.GetFileNameWithoutExtension(relevant.Last());

                return string.Join("/", relevant);
                // returns "products/123/abc123"  ← 
            }
            catch
            {
                return null;
            }
        }
    }
}



