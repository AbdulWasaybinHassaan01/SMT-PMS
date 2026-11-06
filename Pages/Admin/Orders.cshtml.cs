using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechWorldPMS.Models;
using TechWorldPMS.Repositories;

namespace TechWorldPMS.Pages.Admin
{
    [Authorize(Policy = "RequireAdminOrManager")]
    public class OrdersModel : PageModel
    {
        private readonly IOrderRepository _orderRepository;

        public OrdersModel(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public List<Order> Orders { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            Orders = await _orderRepository.GetAllOrdersAsync();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int orderId, string status)
        {
            await _orderRepository.UpdateOrderStatusAsync(orderId, status);
            TempData["SuccessMessage"] = $"Order #{orderId} status updated to '{status}'.";
            return RedirectToPage();
        }
    }
}

