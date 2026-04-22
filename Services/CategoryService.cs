using AurHER.Data;
using AurHER.DTOs.Admin;
using AurHER.DTOs.Store;
using AurHER.Models;
using AurHER.Repositories.Interfaces;
using AurHER.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AurHER.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly AppDbContext _context;

        public CategoryService(ICategoryRepository categoryRepository, AppDbContext context)
        {
            _categoryRepository = categoryRepository;
             _context = context;
        }


        public async Task<List<HomeCategoryDto>> GetPublicCategoriesAsync()
        {
               var categories = await _context.Categories
                .Include(c => c.Products)
                .Where(c => c.Products.Any(p => p.IsActive)) .ToListAsync();
                return categories.Select(c => new HomeCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products.Count(p => p.IsActive)
                })
                .ToList();
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _context.Categories
                    .Include(c => c.Products)
                    .ToListAsync();
            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ProductCount = c.Products?.Count ?? 0
            });
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetCategoryWithProductsAsync(id);
            if (category == null) return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ProductCount = category.Products?.Count ?? 0
            };
        }

        public async Task<bool> CreateCategoryAsync(CreateCategoryDto dto)
        {
            // Check if name already exists
            var exists = await _categoryRepository.CategoryExistsAsync(dto.Name);
            if (exists) return false;

            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            await _categoryRepository.AddAsync(category);
            return true;
        }

        public async Task<bool> UpdateCategoryAsync(UpdateCategoryDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(dto.Id);
            if (category == null) return false;

            category.Name = dto.Name;
            category.Description = dto.Description;

            await _categoryRepository.UpdateAsync(category);
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            await _categoryRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> CategoryExistsAsync(string name)
        {
            return await _categoryRepository.CategoryExistsAsync(name);
        }
    }
}