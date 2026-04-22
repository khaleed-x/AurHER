using AurHER.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AurHER.Controllers
{
    public class TrackController : Controller
    {
        private readonly IOrderTrackingService _trackingService;

        public TrackController(IOrderTrackingService trackingService)
        {
            _trackingService = trackingService;
        }

        public async Task<IActionResult> Index(string? code = null)
        {
            if (string.IsNullOrEmpty(code))
                return View();

            var order = await _trackingService.GetOrderByCodeAsync(code);

            if (order == null)
            {
                ViewBag.Error = "No order found with that confirmation code. Please check and try again.";
                ViewBag.Code = code;
                return View();
            }

            return View(order);
        }
    }
}