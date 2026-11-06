using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechWorldPMS.Models;
using TechWorldPMS.Repositories;

namespace TechWorldPMS.Pages.Customer
{
    [Authorize(Policy = "RequireCustomerOrAdmin")]
    public class CheckoutModel : PageModel
    {
        private readonly ICartRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;

        public CheckoutModel(
            ICartRepository cartRepository,
            IOrderRepository orderRepository,
            IUserRepository userRepository)
        {
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
        }

        public List<CartItem> CartItems { get; set; } = new();
        public decimal TotalAmount => CartItems.Sum(i => i.SubTotal);

        [BindProperty]
        public CheckoutInputModel Input { get; set; } = new();

        public class CheckoutInputModel
        {
            [Required(ErrorMessage = "Full Name is required")]
            [Display(Name = "Full Name")]
            public string CustomerName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Phone Number is required")]
            [Phone(ErrorMessage = "Please enter a valid phone number")]
            [Display(Name = "Phone Number")]
            public string Phone { get; set; } = string.Empty;

            [Required(ErrorMessage = "House / Residential Address is required")]
            [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters")]
            [Display(Name = "House / Street Address")]
            public string ShippingAddress { get; set; } = string.Empty;

            [Required(ErrorMessage = "City is required")]
            [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
            [Display(Name = "City")]
            public string City { get; set; } = string.Empty;

            [Display(Name = "Postal Code")]
            public string? PostalCode { get; set; }

            [Required(ErrorMessage = "Please select a payment method")]
            [Display(Name = "Payment Method")]
            public string PaymentMethod { get; set; } = "Cash on Delivery";

            [Display(Name = "Delivery Instructions / Notes")]
            public string? Notes { get; set; }

            // Simulated card fields (optional validation if Card selected)
            public string? CardNumber { get; set; }
            public string? CardExpiry { get; set; }
            public string? CardCVV { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();
            if (userId <= 0) return RedirectToPage("/Account/Login");

            CartItems = await _cartRepository.GetCartAsync(userId);
            if (!CartItems.Any())
            {
                return RedirectToPage("/Customer/Cart");
            }

            // Pre-fill user profile information
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                Input.CustomerName = user.FullName;
                Input.Phone = user.Phone ?? string.Empty;
                Input.ShippingAddress = user.Address ?? string.Empty;
                Input.City = user.City ?? string.Empty;
                Input.PostalCode = user.PostalCode ?? string.Empty;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = GetCurrentUserId();
            if (userId <= 0) return RedirectToPage("/Account/Login");

            CartItems = await _cartRepository.GetCartAsync(userId);
            if (!CartItems.Any())
            {
                return RedirectToPage("/Customer/Cart");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "customer@techworld.com";

            var order = new Order
            {
                UserId = userId,
                CustomerName = Input.CustomerName.Trim(),
                CustomerEmail = userEmail,
                TotalAmount = TotalAmount,
                Status = "Pending",
                PaymentMethod = Input.PaymentMethod,
                ShippingAddress = Input.ShippingAddress.Trim(),
                City = Input.City.Trim(),
                PostalCode = Input.PostalCode?.Trim(),
                Phone = Input.Phone.Trim(),
                Notes = Input.Notes?.Trim(),
                OrderDate = DateTime.UtcNow
            };

            // Also update customer profile with latest address so they don't have to re-type it
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.Address = Input.ShippingAddress.Trim();
                user.City = Input.City.Trim();
                user.PostalCode = Input.PostalCode?.Trim();
                user.Phone = Input.Phone.Trim();
                await _userRepository.UpdateProfileAsync(user);
            }

            int orderId = await _orderRepository.CreateOrderAsync(order, CartItems);

            TempData["SuccessMessage"] = $"Thank you! Your order #{orderId} has been successfully placed. We will deliver to your address shortly!";
            return RedirectToPage("/Customer/Orders");
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out int id) ? id : 0;
        }
    }
}

