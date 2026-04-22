using AurHER.DTOs.Admin;
using AurHER.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AurHER.Controllers
{
    [Authorize]
    public class CollectionController : Controller
    {
        private readonly ICollectionService _collectionService;

        public CollectionController(ICollectionService collectionService)
        {
            _collectionService = collectionService;
        }


        [AllowAnonymous]
        public async Task<IActionResult> PublicIndex()
        {
            var collections = await _collectionService.GetPublicCollectionsAsync();
            return View(collections);  
        }

        public async Task<IActionResult> Index()
        {
            var collections = await _collectionService.GetAllCollectionsAsync();
            return View(collections);
        }

    
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCollectionDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var exists = await _collectionService.CollectionExistsAsync(dto.Name);
            if (exists)
            {
                ModelState.AddModelError("Name", "A collection with this name already exists");
                return View(dto);
            }

            await _collectionService.CreateCollectionAsync(dto);

            TempData["SuccessMessage"] = $"Collection '{dto.Name}' created successfully!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var collection = await _collectionService.GetCollectionByIdAsync(id);
            if (collection == null)
                return NotFound();

            var dto = new UpdateCollectionDto
            {
                Id = collection.Id,
                Name = collection.Name
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateCollectionDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var exists = await _collectionService.CollectionExistsAsync(dto.Name);
            if (exists)
            {
                ModelState.AddModelError("Name", "A collection with this name already exists");
                return View(dto);
            }

            var result = await _collectionService.UpdateCollectionAsync(dto);
            if (!result)
            {
                ModelState.AddModelError("", "Collection not found or could not be updated");
                return View(dto);
            }

            TempData["SuccessMessage"] = $"Collection '{dto.Name}' updated successfully!";
            return RedirectToAction("Index");
        }

      
        public async Task<IActionResult> Details(int id)
        {
            var collection = await _collectionService.GetCollectionWithProductsAsync(id);
            if (collection == null)
                return NotFound();

            return View(collection);
        }


        [HttpPost]
        public async Task<IActionResult> AddProduct(AddProductToCollectionDto dto)
        {
            // Guard against no product selected
            if (dto.ProductId == 0)
            {
                TempData["ErrorMessage"] = "Please select a product to add!";
                return RedirectToAction("Details", new { id = dto.CollectionId });
            }

            await _collectionService.AddProductToCollectionAsync(dto);
            TempData["SuccessMessage"] = "Product added to collection!";
            return RedirectToAction("Details", new { id = dto.CollectionId });
        }


        [HttpPost]
        public async Task<IActionResult> RemoveProduct(int collectionId, int productId)
        {
            await _collectionService.RemoveProductFromCollectionAsync(collectionId, productId);
            TempData["SuccessMessage"] = "Product removed from collection!";
            return RedirectToAction("Details", new { id = collectionId });
        }

     
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _collectionService.DeleteCollectionAsync(id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Collection not found or could not be deleted";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "Collection deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}