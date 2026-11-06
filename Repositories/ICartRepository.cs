using TechWorldPMS.Models;

namespace TechWorldPMS.Repositories
{
    public interface ICartRepository
    {
        Task<List<CartItem>> GetCartAsync(int userId);
        Task<int> GetCartCountAsync(int userId);
        Task<bool> AddToCartAsync(int userId, int productId, int quantity);
        Task<bool> UpdateQuantityAsync(int cartItemId, int userId, int quantity);
        Task<bool> RemoveItemAsync(int cartItemId, int userId);
        Task<bool> ClearCartAsync(int userId);
    }
}

