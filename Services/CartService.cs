using AurHER.Data;
using AurHER.DTOs.Store;
using AurHER.Models;
using AurHER.Repositories.Interfaces;
using AurHER.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AurHER.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly AppDbContext _context;

        public CartService(
            ICartRepository cartRepository, 
            AppDbContext context)
        {
            _cartRepository = cartRepository;
            _context = context;
        }

        // Get or create cart for session
        private async Task<Cart> GetOrCreateCartAsync(string sessionId)
        {
            var cart = await _cartRepository.GetCartBySessionAsync(sessionId);
            if (cart == null)
            {
                cart = new Cart
                {
                    SessionId = sessionId,
                    CreatedAt = DateTime.UtcNow
                };
                await _cartRepository.AddAsync(cart);
            }
            return cart;
        }

        public async Task<CartDto> GetCartAsync(string sessionId)
        {
            var cart = await _cartRepository.GetCartWithItemsAsync(sessionId);

            if (cart == null)
                {
                    return new CartDto
                    {
                        CartId = 0,
                        Items = new List<CartItemDto>(),
                        Subtotal = 0,
                        TotalItems = 0
                    };
                }

            var items = cart.Items.Select(i => new CartItemDto
            {
                CartItemId = i.Id,
                VariantId = i.ProductVariantId,
                ProductName = i.ProductVariant?.Product?.Name ?? "",
                PrimaryImage = i.ProductVariant?.Product?.Images?
                    .FirstOrDefault(img => img.Isprimary)?.ImageUrl
                    ?? i.ProductVariant?.Product?.Images?.FirstOrDefault()?.ImageUrl,
                Size = i.ProductVariant?.Size ?? "",
                Color = i.ProductVariant?.Color ?? "",
                Price = i.ProductVariant?.Price ?? 0,
                Quantity = i.Quantity,
                StockQuantity = i.ProductVariant?.StockQuantity ?? 0
            }).ToList()?? new List<CartItemDto>();;

            return new CartDto
            {
                CartId = cart.Id,
                Items = items,
                Subtotal = items.Sum(i => i.Subtotal),
                TotalItems = items.Sum(i => i.Quantity)
            };
        }

        public async Task<int> GetCartCountAsync(string sessionId)
        {
            var cart = await _cartRepository.GetCartBySessionAsync(sessionId);
            if (cart == null) return 0;

            return await _context.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .SumAsync(ci => ci.Quantity);
        }

        public async Task<AddToCartResult> AddToCartAsync(string sessionId, AddToCartDto dto)
        {
            // 1. Check stock availability
            var variant = await _context.ProductVariants.FindAsync(dto.VariantId);
            if (variant == null)
            {
                return new AddToCartResult { Success = false, Message = "Product variant not found" };
            }

            if (variant.StockQuantity < dto.Quantity)
            {
                return new AddToCartResult 
                { 
                    Success = false, 
                    Message = $"Only {variant.StockQuantity} left in stock. Please reduce quantity."
                };
            }

            // 2. Proceed with adding to cart
            var cart = await GetOrCreateCartAsync(sessionId);
            await _cartRepository.AddItemToCartAsync(cart.Id, dto.VariantId, dto.Quantity);

            return new AddToCartResult { Success = true, Message = "Item added to cart" };
        }

        public async Task UpdateQuantityAsync(string sessionId, int cartItemId, int quantity)
        {
            
            await _cartRepository.UpdateItemQuantityAsync(cartItemId, quantity);
        }

        public async Task RemoveFromCartAsync(string sessionId, int cartItemId)
        {
            await _cartRepository.RemoveItemFromCartAsync(cartItemId);
        }

        public async Task ClearCartAsync(string sessionId)
        {
            var cart = await _cartRepository.GetCartBySessionAsync(sessionId);
            if (cart != null)
                await _cartRepository.ClearCartAsync(cart.Id);
        }

        
    }
}