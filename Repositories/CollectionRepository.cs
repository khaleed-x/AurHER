using Microsoft.EntityFrameworkCore;
using AurHER.Data;
using AurHER.Models;
using AurHER.Repositories.Interfaces;

namespace AurHER.Repositories
{
    public class CollectionRepository : ICollectionRepository
    {
        private readonly AppDbContext _context;

        public CollectionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Collection collection)
        {
            await _context.Collections.AddAsync(collection);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Collection collection)
        {
            _context.Collections.Update(collection);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var collection = await GetByIdAsync(id);
            if (collection != null)
            {
                _context.Collections.Remove(collection);
                await _context.SaveChangesAsync();
            }
        }
        // Get all collections, no relations
        public async Task<IEnumerable<Collection>> GetAllAsync()
        {
            return await _context.Collections.ToListAsync();
        }

        // Get all collections with their products
        public async Task<IEnumerable<Collection>> GetAllWithProductsAsync()
        {
            return await _context.Collections
                .Include(c => c.Products)
                    .ThenInclude(cp => cp.Product)
                        .ThenInclude(p => p.Images)
                .ToListAsync();
        }

        // Get single collection by ID, no relations
        public async Task<Collection?> GetByIdAsync(int id)
        {
            return await _context.Collections.FindAsync(id);
        }

        // Get single collection with all its products
        public async Task<Collection?> GetCollectionWithProductsAsync(int id)
        {
            return await _context.Collections
                .Include(c => c.Products)
                    .ThenInclude(cp => cp.Product)
                             .ThenInclude(p=> p.Images)
                                //second ef-core chain........ selecting 2 child from same parent (Product)
                                .Include(c => c.Products)
                                .ThenInclude(cp => cp.Product)
                                    .ThenInclude(p => p.Variants)
    
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // Check if a product already exists in a collection
        public async Task<bool> ProductExistsInCollectionAsync(int collectionId, int productId)
        {
            return await _context.CollectionProducts
                .AnyAsync(cp =>
                    cp.CollectionId == collectionId &&
                    cp.ProductId == productId);
        }

        // Add a product to a collection
        public async Task AddProductToCollectionAsync(int collectionId, int productId)
        {
            // First check if it already exists
            var exists = await ProductExistsInCollectionAsync(collectionId, productId);
            if (!exists)
            {
                var collectionProduct = new CollectionProduct
                {
                    CollectionId = collectionId,
                    ProductId = productId
                };

                await _context.CollectionProducts.AddAsync(collectionProduct);
                await _context.SaveChangesAsync();
            }
        }

        // Remove a product from a collection
        public async Task RemoveProductFromCollectionAsync(int collectionId, int productId)
        {
            var collectionProduct = await _context.CollectionProducts
                .FirstOrDefaultAsync(cp =>
                    cp.CollectionId == collectionId &&
                    cp.ProductId == productId);

            if (collectionProduct != null)
            {
                _context.CollectionProducts.Remove(collectionProduct);
                await _context.SaveChangesAsync();
            }
        }

        
    }
}
