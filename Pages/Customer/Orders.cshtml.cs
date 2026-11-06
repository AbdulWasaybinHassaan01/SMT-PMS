using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechWorldPMS.Models;
using TechWorldPMS.Repositories;

namespace TechWorldPMS.Pages.Customer
{
    [Authorize(Policy = "RequireCustomerOrAdmin")]
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

        public async Task<IActionResult> OnGetAsync()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claim, out int userId))
            {
                return RedirectToPage("/Account/Login");
            }

            Orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            return Page();
        }
    }
}

