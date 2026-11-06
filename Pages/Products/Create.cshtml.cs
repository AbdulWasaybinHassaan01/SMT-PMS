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
    public class CreateModel : PageModel
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFileService _fileService;

        public CreateModel(
            IProductRepository productRepository, 
            ICategoryRepository categoryRepository,
            IFileService fileService)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _fileService = fileService;
        }

        [BindProperty]
        public ProductInputModel Input { get; set; } = new();

        [BindProperty]
        public IFormFile? ProductImageFile { get; set; }

        public SelectList CategoryOptions { get; set; } = default!;

        public class ProductInputModel
        {
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
        }

        public async Task<IActionResult> OnGetAsync()
        {
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

            // Handle image upload or fallback
            string? imageUrl = "/images/products/default-product.svg";
            if (ProductImageFile != null && ProductImageFile.Length > 0)
            {
                imageUrl = await _fileService.SaveProductImageAsync(ProductImageFile, imageUrl);
            }

            var product = new Product
            {
                Name = Input.Name.Trim(),
                CategoryId = Input.CategoryId,
                Price = Input.Price,
                StockQuantity = Input.StockQuantity,
                Description = Input.Description?.Trim(),
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            };

            int newId = await _productRepository.AddAsync(product);

            TempData["SuccessMessage"] = $"Product '{product.Name}' added successfully!";
            return RedirectToPage("/Products/Index");
        }

        private async Task PopulateCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            CategoryOptions = new SelectList(categories, "CategoryId", "CategoryName");
        }
    }
}

