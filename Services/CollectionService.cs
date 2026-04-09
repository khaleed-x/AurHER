
using AurHER.Data;
using AurHER.DTOs.Admin;
using AurHER.DTOs.Store;
using AurHER.Models;
using AurHER.Repositories.Interfaces;
using AurHER.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AurHER.Services
{
    public class CollectionService : ICollectionService
    {
        private readonly ICollectionRepository _collectionRepository;
        private readonly IProductRepository _productRepository;
        private readonly AppDbContext _context;

        public CollectionService(
            ICollectionRepository collectionRepository,
            IProductRepository productRepository,
            AppDbContext context)
        {
            _collectionRepository = collectionRepository;
            _productRepository = productRepository;
            _context = context;
        }

        public async Task<bool> CollectionExistsAsync(string name)
        {
            var collections = await _collectionRepository.GetAllAsync();
            return collections.Any(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<List<HomeCollectionDto>> GetPublicCollectionsAsync()
        {
            



               var collections = await _context.Collections
                .Include(c => c.Products)
                    .ThenInclude(cp => cp.Product)
                        .ThenInclude(p => p.Images)
                .Include(c => c.Products)
                    .ThenInclude(cp => cp.Product)
                        .ThenInclude(p => p.Variants)
                .Where(c => c.Products.Any(cp => cp.Product.IsActive)).ToListAsync();
                return collections.Select(c => new HomeCollectionDto
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
                        }).ToList(),
                })
                .ToList();
    }

        public async Task<IEnumerable<CollectionDto>> GetAllCollectionsAsync()
        {
            
                var collections = await _context.Collections
                    .Include(c => c.Products)
                    .ToListAsync();

                return collections.Select(c => new CollectionDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProductCount = c.Products?.Count ?? 0
                });
            
        }

        public async Task<CollectionDto?> GetCollectionByIdAsync(int id)
        {
            var collection = await _collectionRepository.GetByIdAsync(id);
            if (collection == null) return null;

            return new CollectionDto
            {
                Id = collection.Id,
                Name = collection.Name,
                ProductCount = collection.Products?.Count ?? 0
            };
        }

        public async Task<CollectionDetailDto?> GetCollectionWithProductsAsync(int id)
        {
            var collection = await _collectionRepository.GetCollectionWithProductsAsync(id);
            if (collection == null) return null;

            // Products already in collection
            
            var productsInCollection = collection.Products?.Select(cp => new CollectionProductItemDto
            {
                ProductId = cp.ProductId,
                ProductName = cp.Product?.Name ?? "",
                PrimaryImage = cp.Product?.Images?
                    .FirstOrDefault(i => i.Isprimary)?.ImageUrl,
                VariantCount = cp.Product?.Variants?.Count ?? 0,
                CategoryName = cp.Product?.Category?.Name
            }).ToList() ?? new List<CollectionProductItemDto>();

            // IDs of products already in collection
            var existingProductIds = productsInCollection
                .Select(p => p.ProductId)
                .ToHashSet();

            // Products NOT yet in this collection
            var allProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && !existingProductIds.Contains(p.Id))
                .Select(p => new AvailableProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    CategoryName = p.Category != null ? p.Category.Name : null
                })
                .ToListAsync();

            return new CollectionDetailDto
            {
                Id = collection.Id,
                Name = collection.Name,
                Products = productsInCollection,
                AvailableProducts = allProducts
            };
        }

        public async Task<bool> CreateCollectionAsync(CreateCollectionDto dto)
        {
            var collection = new Collection
            {
                Name = dto.Name
            };

            await _collectionRepository.AddAsync(collection);
            return true;
        }

        public async Task<bool> UpdateCollectionAsync(UpdateCollectionDto dto)
        {
            var collection = await _collectionRepository.GetByIdAsync(dto.Id);
            if (collection == null) return false;

            collection.Name = dto.Name;
            await _collectionRepository.UpdateAsync(collection);
            return true;
        }

        public async Task<bool> DeleteCollectionAsync(int id)
        {
            var collection = await _collectionRepository.GetByIdAsync(id);
            if (collection == null) return false;

            await _collectionRepository.DeleteAsync(id);
            return true;
        }

        public async Task AddProductToCollectionAsync(AddProductToCollectionDto dto)
        {
            await _collectionRepository.AddProductToCollectionAsync(
                dto.CollectionId, dto.ProductId);
        }

        public async Task RemoveProductFromCollectionAsync(int collectionId, int productId)
        {
            await _collectionRepository.RemoveProductFromCollectionAsync(
                collectionId, productId);
        }
    }
}