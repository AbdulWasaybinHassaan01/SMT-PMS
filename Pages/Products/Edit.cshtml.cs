using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechWorldPMS.Models;
using TechWorldPMS.Repositories;
using TechWorldPMS.Services;

namespace TechWorldPMS.Pages.Products
{
    [Authorize(Policy = "RequireAdminOrManager")]
    public class EditModel : PageModel
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFileService _fileService;

        public EditModel(
            IProductRepository productRepository, 
            ICategoryRepository categoryRepository,
            IFileService fileService)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _fileService = fileService;
        }

        [BindProperty]
        public ProductEditInputModel Input { get; set; } = new();

        [BindProperty]
        public IFormFile? NewProductImageFile { get; set; }

        public SelectList CategoryOptions { get; set; } = default!;

        public class ProductEditInputModel
        {
            public int ProductId { get; set; }

            [Required(ErrorMessage = "Product Name is required")]
            [StringLength(150, MinimumLength = 2, ErrorMessage = "Product Name must be between 2 and 150 characters")]
            [Display(Name = "Product Name")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Category is required")]
            [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
            [Display(Name = "Category")]
            public int CategoryId { get; set; }

            [Required(ErrorMessage = "Price is required")]
            [Range(0.01, 10000000.00, ErrorMessage = "Price must be greater than 0")]
            [Display(Name = "Price (PKR)")]
            public decimal Price { get; set; }

            [Required(ErrorMessage = "Stock quantity is required")]
            [Range(0, 100000, ErrorMessage = "Stock Quantity must be 0 or more")]
            [Display(Name = "Stock Quantity")]
            public int StockQuantity { get; set; }

            [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
            [Display(Name = "Description")]
            public string? Description { get; set; }

            public string? CurrentImageUrl { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            Input = new ProductEditInputModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                CategoryId = product.CategoryId,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Description = product.Description,
                CurrentImageUrl = product.ImageUrl
            };

            await PopulateCategoriesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync();
                return Page();
            }

            string? finalImageUrl = Input.CurrentImageUrl;
            if (NewProductImageFile != null && NewProductImageFile.Length > 0)
            {
                string? uploadedUrl = await _fileService.SaveProductImageAsync(NewProductImageFile);
                if (!string.IsNullOrEmpty(uploadedUrl))
                {
                    finalImageUrl = uploadedUrl;
                }
            }

            var product = new Product
            {
                ProductId = Input.ProductId,
                Name = Input.Name.Trim(),
                CategoryId = Input.CategoryId,
                Price = Input.Price,
                StockQuantity = Input.StockQuantity,
                Description = Input.Description?.Trim(),
                ImageUrl = finalImageUrl,
                UpdatedAt = DateTime.UtcNow
            };

            bool updated = await _productRepository.UpdateAsync(product);
            if (!updated)
            {
                ModelState.AddModelError(string.Empty, "Could not update the product in the database.");
                await PopulateCategoriesAsync();
                return Page();
            }

            TempData["SuccessMessage"] = $"Product '{product.Name}' updated successfully!";
            return RedirectToPage("/Products/Index");
        }

        private async Task PopulateCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            CategoryOptions = new SelectList(categories, "CategoryId", "CategoryName", Input.CategoryId);
        }
    }
}

