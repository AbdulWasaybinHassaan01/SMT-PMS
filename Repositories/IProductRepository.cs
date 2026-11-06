using TechWorldPMS.Models;

namespace TechWorldPMS.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(string? searchTerm = null, int? categoryId = null);
        Task<IEnumerable<Product>> GetRecentAsync(int count = 5);
        Task<Product?> GetByIdAsync(int id);
        Task<int> AddAsync(Product product);
        Task<bool> UpdateAsync(Product product);
        Task<bool> DeleteAsync(int id);
        Task<DashboardStats> GetDashboardStatsAsync();
        Task<bool> UpdateStockAsync(int productId, int quantityChange);
    }
}

