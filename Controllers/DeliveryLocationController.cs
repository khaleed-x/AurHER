using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AurHER.Models;
using AurHER.Services.Interfaces;

namespace AurHER.Controllers.Admin
{
    [Authorize]
    public class DeliveryLocationController : Controller
    {
        private readonly IDeliveryLocationService _service;

        public DeliveryLocationController(IDeliveryLocationService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var locations = await _service.GetAllLocationsAsync();
            return View(locations);
        }

  
        public IActionResult Create()
        {
            return View();
        }

    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeliveryLocation location)
        {
            if (!ModelState.IsValid)
            {
                return View(location);
            }

            await _service.CreateLocationAsync(location);
            TempData["Success"] = "Delivery location added successfully.";
            return RedirectToAction(nameof(Index));
        }

       
        public async Task<IActionResult> Edit(int id)
        {
            var location = await _service.GetLocationByIdAsync(id);
            if (location == null)
            {
                return NotFound();
            }
            return View(location);
        }

 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DeliveryLocation location)
        {
            if (id != location.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(location);
            }

            await _service.UpdateLocationAsync(location);
            TempData["Success"] = "Delivery location updated successfully.";
            return RedirectToAction(nameof(Index));
        }

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteLocationAsync(id);
            TempData["Success"] = "Delivery location deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}