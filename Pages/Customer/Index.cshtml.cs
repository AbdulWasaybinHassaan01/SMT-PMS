using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechWorldPMS.Models;
using TechWorldPMS.Repositories;

namespace TechWorldPMS.Pages.Customer
{
    [Authorize(Policy = "RequireCustomerOrAdmin")]
    public class IndexModel : PageModel
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICartRepository _cartRepository;

        public IndexModel(
            IProductRepository productRepository, 
            ICategoryRepository categoryRepository,
            ICartRepository cartRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _cartRepository = cartRepository;
        }

        public IEnumerable<Product> Products { get; set; } = Enumerable.Empty<Product>();
        public IEnumerable<Category> Categories { get; set; } = Enumerable.Empty<Category>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _categoryRepository.GetAllAsync();
            Products = await _productRepository.GetAllAsync(SearchTerm, CategoryId);
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int productId, int quantity)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToPage("/Account/Login");
            }

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToPage();
            }

            if (product.StockQuantity <= 0)
            {
                TempData["ErrorMessage"] = $"Sorry, '{product.Name}' is out of stock!";
                return RedirectToPage();
            }

            if (quantity <= 0) quantity = 1;
            if (quantity > product.StockQuantity) quantity = product.StockQuantity;

            await _cartRepository.AddToCartAsync(userId, productId, quantity);
            TempData["SuccessMessage"] = $"Added {quantity}x '{product.Name}' to your shopping cart!";

            return RedirectToPage();
        }
    }
}

