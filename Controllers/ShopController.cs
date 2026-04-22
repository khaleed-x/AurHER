using AurHER.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AurHER.Controllers
{
    public class ShopController : Controller
    {
        private readonly IShopService _shopService;

        public ShopController(IShopService shopService)
        {
            _shopService = shopService;
        }

        public async Task<IActionResult> Index(
            string? category = null,
            string? search = null,
            string? sort = null)
        {
            var products = await _shopService.GetAllProductsAsync(category, search, sort);

            ViewBag.Category = category;
            ViewBag.Search = search;
            ViewBag.Sort = sort;

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _shopService.GetProductDetailAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }
    }
}