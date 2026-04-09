using AurHER.DTOs.Admin;
using AurHER.Repositories.Interfaces;
using AurHER.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AurHER.Data;

namespace AurHER.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(
            IProductService productService,
            ICategoryRepository categoryRepository)
        {
            _productService = productService;
            _categoryRepository = categoryRepository;
        }

        // ── Helper: Load categories for dropdowns ──
        private async Task LoadCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
        }

       
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesAsync();
            return View();
        }

       
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return View(dto);
            }

            await _productService.CreateProductAsync(dto);
            TempData["SuccessMessage"] = $"Product '{dto.Name}' created successfully!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductForEditAsync(id);
            if (product == null) return NotFound();

            await LoadCategoriesAsync();
            return View(product);
        }

      
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return View(dto);
            }

            var result = await _productService.UpdateProductAsync(dto);
            if (!result)
            {
                ModelState.AddModelError("", "Product not found or could not be updated");
                await LoadCategoriesAsync();
                return View(dto);
            }

            TempData["SuccessMessage"] = $"Product '{dto.Name}' updated successfully!";
            return RedirectToAction("Index");
        }

        
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductWithDetailsAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

      
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            TempData[result ? "SuccessMessage" : "ErrorMessage"] = result
                ? "Product deleted successfully!"
                : "Product could not be deleted";

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            await _productService.ToggleProductStatusAsync(id);
            return RedirectToAction("Index");
        }

      

       
        public IActionResult AddVariant(int productId)
        {
            return View(new CreateVariantDto { ProductId = productId });
        }

      
        [HttpPost]
        public async Task<IActionResult> AddVariant(CreateVariantDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _productService.AddVariantAsync(dto);
            if (!result)
            {
                ModelState.AddModelError("SKU", "A variant with this SKU already exists");
                return View(dto);
            }

            TempData["SuccessMessage"] = "Variant added successfully!";
            return RedirectToAction("Details", new { id = dto.ProductId });
        }

     
        public async Task<IActionResult> EditVariant(int id)
        {
            var variant = await _productService.GetVariantForEditAsync(id);
            if (variant == null) return NotFound();

            return View(variant);
        }

       
        [HttpPost]
        public async Task<IActionResult> EditVariant(UpdateVariantDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _productService.UpdateVariantAsync(dto);
            if (!result)
            {
                ModelState.AddModelError("SKU", "A variant with this SKU already exists");
                return View(dto);
            }

            TempData["SuccessMessage"] = "Variant updated successfully!";
            return RedirectToAction("Details", new { id = dto.ProductId });
        }

       
        [HttpPost]
        public async Task<IActionResult> DeleteVariant(int id, int productId)
        {
            await _productService.DeleteVariantAsync(id);
            TempData["SuccessMessage"] = "Variant deleted successfully!";
            return RedirectToAction("Details", new { id = productId });
        }

        

        [HttpPost]
        public async Task<IActionResult> AddImage(int productId, IFormFile file, bool isPrimary)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select an image file!";
                return RedirectToAction("Details", new { id = productId });
            }

            var result = await _productService.AddImageAsync(productId, file, isPrimary);
            TempData[result ? "SuccessMessage" : "ErrorMessage"] = result
                ? "Image uploaded successfully!"
                : "Invalid file type. Only JPG, PNG and WEBP are allowed.";

            return RedirectToAction("Details", new { id = productId });
        }

       
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id, int productId)
        {
            await _productService.DeleteImageAsync(id);
            TempData["SuccessMessage"] = "Image deleted successfully!";
            return RedirectToAction("Details", new { id = productId });
        }

    
        [HttpPost]
        public async Task<IActionResult> SetPrimaryImage(int id, int productId)
        {
            await _productService.SetPrimaryImageAsync(id);
            TempData["SuccessMessage"] = "Primary image updated!";
            return RedirectToAction("Details", new { id = productId });
        }
    }
}