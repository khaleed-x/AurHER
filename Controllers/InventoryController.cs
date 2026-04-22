using AurHER.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AurHER.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Task<IActionResult> Index(string filter = "all")
        {
            var inventory = filter switch
            {
                "low" => await _inventoryService.GetLowStockAsync(),
                "out" => await _inventoryService.GetOutOfStockAsync(),
                _ => await _inventoryService.GetInventoryAsync()
            };

            ViewBag.Filter = filter;
            ViewBag.TotalCount = (await _inventoryService.GetInventoryAsync()).Count();
            ViewBag.LowStockCount = (await _inventoryService.GetLowStockAsync()).Count();
            ViewBag.OutOfStockCount = (await _inventoryService.GetOutOfStockAsync()).Count();

            return View(inventory);
        }
    }
}