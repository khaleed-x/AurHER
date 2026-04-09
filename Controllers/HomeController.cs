using AurHER.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AurHER.Controllers
{
    public class HomeController : Controller
    {
        private readonly IShopService _shopService;

        public HomeController(IShopService shopService)
        {
            _shopService = shopService;
        }

        public async Task<IActionResult> Index()
        {
            var homeData = await _shopService.GetHomePageDataAsync();
            return View(homeData);
        }
    }
}
