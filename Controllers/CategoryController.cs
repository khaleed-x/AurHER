using AurHER.DTOs.Admin;
using AurHER.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AurHER.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> PublicIndex()
        {
            var categories = await _categoryService.GetPublicCategoriesAsync();
            return View(categories);  // Uses Views/Category/PublicIndex.cshtml
        }

       
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

     
        public IActionResult Create()
        {
            return View();
        }

       
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

           
            var exists = await _categoryService.CategoryExistsAsync(dto.Name);
            if (exists)
            {
                ModelState.AddModelError("Name", "A category with this name already exists");
                return View(dto);
            }

            var result = await _categoryService.CreateCategoryAsync(dto);
            if (!result)
            {
                ModelState.AddModelError("", "Something went wrong, please try again");
                return View(dto);
            }

            TempData["SuccessMessage"] = $"Category '{dto.Name}' created successfully!";
            return RedirectToAction("Index");
        }

       
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();

            var dto = new UpdateCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View(dto);
        }

       
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _categoryService.UpdateCategoryAsync(dto);
            if (!result)
            {
                ModelState.AddModelError("", "Category not found or could not be updated");
                return View(dto);
            }

            TempData["SuccessMessage"] = $"Category '{dto.Name}' updated successfully!";
            return RedirectToAction("Index");
        }

        
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Category not found or could not be deleted";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "Category deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}