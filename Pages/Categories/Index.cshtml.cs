using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechWorldPMS.Models;
using TechWorldPMS.Repositories;

namespace TechWorldPMS.Pages.Categories
{
    [Authorize(Policy = "RequireAdminOrManager")]
    public class IndexModel : PageModel
    {
        private readonly ICategoryRepository _categoryRepository;

        public IndexModel(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public IEnumerable<Category> Categories { get; set; } = Enumerable.Empty<Category>();

        [BindProperty]
        public CategoryInputModel NewCategory { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class CategoryInputModel
        {
            [Required(ErrorMessage = "Category Name is required")]
            [StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters")]
            public string CategoryName { get; set; } = string.Empty;

            [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters")]
            public string? Description { get; set; }

            public string ColorBadge { get; set; } = "primary";
        }

        public async Task OnGetAsync()
        {
            Categories = await _categoryRepository.GetAllAsync();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = await _categoryRepository.GetAllAsync();
                return Page();
            }

            var category = new Category
            {
                CategoryName = NewCategory.CategoryName.Trim(),
                Description = NewCategory.Description?.Trim(),
                ColorBadge = NewCategory.ColorBadge
            };

            int id = await _categoryRepository.AddAsync(category);
            if (id > 0)
            {
                TempData["SuccessMessage"] = $"Category '{category.CategoryName}' added successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not add category. Name might already exist.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            bool deleted = await _categoryRepository.DeleteAsync(id);
            if (deleted)
            {
                TempData["SuccessMessage"] = "Category deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not delete category.";
            }

            return RedirectToPage();
        }
    }
}

