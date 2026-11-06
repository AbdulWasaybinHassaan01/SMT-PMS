using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechWorldPMS.Models;
using TechWorldPMS.Repositories;

namespace TechWorldPMS.Pages.Products
{
    [Authorize(Policy = "RequireAdminOrManager")]
    public class IndexModel : PageModel
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public IndexModel(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public IEnumerable<Product> Products { get; set; } = Enumerable.Empty<Product>();
        public IEnumerable<Category> Categories { get; set; } = Enumerable.Empty<Category>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _categoryRepository.GetAllAsync();
            Products = await _productRepository.GetAllAsync(SearchTerm, CategoryId);
        }
    }
}

