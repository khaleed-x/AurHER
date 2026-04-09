using AurHER.DTOs.Store;
using AurHER.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AurHER.Controllers
{
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            
            var cart = await _cartService.GetCartAsync(SessionId);
            return View(cart);
        }

        [HttpGet]
        public async Task<IActionResult> Count()
        {
            var count = await _cartService.GetCartCountAsync(SessionId);
            return Json(new { count });
        }

        [HttpPost]
        public async Task<IActionResult> Add(int variantId, int quantity = 1)
        {
            if (variantId <= 0)
                return Json(new { success = false, message = "Invalid product selected" });

            var result = await _cartService.AddToCartAsync(SessionId, new AddToCartDto
            {
                VariantId = variantId,
                Quantity = quantity
            });

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

            var count = await _cartService.GetCartCountAsync(SessionId);
            return Json(new { success = true, cartCount = count });
        }

        [HttpPost]
        public async Task<IActionResult> Update(int cartItemId, int quantity)
        {
            if (quantity <= 0)
            {
                await _cartService.RemoveFromCartAsync(SessionId, cartItemId);
            }
            else
            {
                await _cartService.UpdateQuantityAsync(SessionId, cartItemId, quantity);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            await _cartService.RemoveFromCartAsync(SessionId, cartItemId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            await _cartService.ClearCartAsync(SessionId);
            return RedirectToAction("Index");
        }
        
    }
}