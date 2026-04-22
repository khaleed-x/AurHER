using Microsoft.EntityFrameworkCore;
using AurHER.Data;
using AurHER.Models;
using AurHER.Repositories.Interfaces;

namespace AurHER.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        
        public async Task AddAsync(Cart cart)
        {
            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Cart cart)
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var cart = await GetByIdAsync(id);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<Cart>> GetAllAsync()
        {
            return await _context.Carts.ToListAsync();
        }

        public async Task<Cart?> GetByIdAsync(int id)
        {
            return await _context.Carts.FindAsync(id);
        }

        // Get cart by SessionId
        public async Task<Cart?> GetCartBySessionAsync(string sessionId)
        {
            return await _context.Carts
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);
        }

        // Get cart with all its items, variant details and product info
        public async Task<Cart?> GetCartWithItemsAsync(string sessionId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(v => v.Product)
                            .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);
        }

        // Get a specific item in a cart
        public async Task<CartItem?> GetCartItemAsync(int cartId, int productVariantId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(ci =>
                    ci.CartId == cartId &&
                    ci.ProductVariantId == productVariantId);
        }

       
        public async Task AddItemToCartAsync(int cartId, int productVariantId, int quantity)
        {
            var existingItem = await GetCartItemAsync(cartId, productVariantId);

            if (existingItem != null)
            {
                
                existingItem.Quantity += quantity;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                // If it doesn't exist, add it as a new cart item
                var cartItem = new CartItem
                {
                    CartId = cartId,
                    ProductVariantId = productVariantId,
                    Quantity = quantity
                };
                await _context.CartItems.AddAsync(cartItem);
            }

            await _context.SaveChangesAsync();
        }

     
        public async Task RemoveItemFromCartAsync(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        // Update quantity of a specific cart item
        public async Task UpdateItemQuantityAsync(int cartItemId, int quantity)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                _context.CartItems.Update(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        
        public async Task ClearCartAsync(int cartId)
        {
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

    }
}