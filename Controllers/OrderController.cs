using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AurHER.Services.Interfaces;
using AurHER.Models.Enums;

namespace AurHER.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

            
        public async Task<IActionResult> Index(
            string? search = null, 
            OrderStatus? status = null,
            PaymentStatus? paymentStatus = null)
        {
            var orders = await _orderService.GetAllOrdersAsync(search, status, paymentStatus);
            
            // Pass filter values to view for preserving form state
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentPaymentStatus = paymentStatus;
            
            return View(orders);
        }

        
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderWithDetailsAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
        {
            var result = await _orderService.UpdateOrderStatusAsync(orderId, status);
            
            if (!result)
            {
                TempData["Error"] = "Unable to update order status. order may not exist or selected status is invalid.";
                return RedirectToAction(nameof(Details), new { id = orderId });
            }

            TempData["Success"] = $"Order status updated to {status}.";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        
    }
}