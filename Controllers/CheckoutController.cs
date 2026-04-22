using AurHER.DTOs.Store;
using AurHER.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AurHER.Models;
using System.Collections.Generic;

namespace AurHER.Controllers
{
    public class CheckoutController : BaseController
    {
     private readonly ICartService _cartService;
    private readonly ICheckoutService _checkoutService;
    private readonly IDeliveryLocationService _deliveryLocationService;

    public CheckoutController(
        ICartService cartService,
        ICheckoutService checkoutService,
        IDeliveryLocationService deliveryLocationService)
    {
        _cartService = cartService;
        _checkoutService = checkoutService;
        _deliveryLocationService = deliveryLocationService;
    }

        
    public async Task<IActionResult> Index()
    {
        var cart = await _cartService.GetCartAsync(SessionId);

        if (!cart.Items.Any())
            return RedirectToAction("Index", "Cart");

        var locations = await _deliveryLocationService.GetAllLocationsAsync();

        var model = new CheckoutSummaryDto
        {
            Cart = cart,
            Form = new CheckoutDto(),
            DeliveryLocations = locations
        };

        return View(model);
    }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([Bind(Prefix = "Form")] CheckoutDto dto)
        {
            if (!ModelState.IsValid)
            {
                var cart = await _cartService.GetCartAsync(SessionId);
                var locations = await _deliveryLocationService.GetAllLocationsAsync();
                return View("Index", new CheckoutSummaryDto
                {
                    Cart = cart,
                    Form = dto,
                    DeliveryLocations = locations ?? new List<DeliveryLocation>()
                });
            }

           var (orderId, errorMessage)  = await _checkoutService.PlaceOrderAsync(SessionId, dto);

            if (orderId == null)
            {
                 TempData["ErrorMessage"] = errorMessage ?? "Unable to place order. Please try again.";
                  return RedirectToAction("Index", "Checkout");
            }
               TempData["OrderId"] = orderId;
                return RedirectToAction("ProcessPayment");


        }

        [HttpGet]  
        public IActionResult ProcessPayment()
        {
            var orderId = TempData["OrderId"] as int?;
            if (orderId == null) 
                return RedirectToAction("Index", "Cart");
            
            return RedirectToAction("Initialize", "Payment", new { orderId = orderId });
        }

        
    }
}