using TechWorldPMS.Models;

namespace TechWorldPMS.Repositories
{
    public interface IOrderRepository
    {
        Task<int> CreateOrderAsync(Order order, List<CartItem> items);
        Task<List<Order>> GetOrdersByUserIdAsync(int userId);
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
    }
}

