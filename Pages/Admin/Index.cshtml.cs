using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechWorldPMS.Models;
using TechWorldPMS.Repositories;

namespace TechWorldPMS.Pages.Admin
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

        public DashboardStats Stats { get; set; } = new();
        public IEnumerable<Product> RecentProducts { get; set; } = Enumerable.Empty<Product>();

        public async Task OnGetAsync()
        {
            Stats = await _productRepository.GetDashboardStatsAsync();
            RecentProducts = await _productRepository.GetRecentAsync(5);
        }
    }
}

