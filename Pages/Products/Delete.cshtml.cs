using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechWorldPMS.Models;
using TechWorldPMS.Repositories;
using TechWorldPMS.Services;

namespace TechWorldPMS.Pages.Products
{
    [Authorize(Policy = "RequireAdminOrManager")]
    public class DeleteModel : PageModel
    {
        private readonly IProductRepository _productRepository;
        private readonly IFileService _fileService;

        public DeleteModel(IProductRepository productRepository, IFileService fileService)
        {
            _productRepository = productRepository;
            _fileService = fileService;
        }

        public Product Product { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            Product = product;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null)
            {
                // Delete associated uploaded file if custom
                _fileService.DeleteImage(product.ImageUrl);
                await _productRepository.DeleteAsync(id);

                TempData["SuccessMessage"] = $"Product '{product.Name}' was deleted successfully.";
            }

            return RedirectToPage("/Products/Index");
        }
    }
}

