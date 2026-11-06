using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechWorldPMS.Models;
using TechWorldPMS.Repositories;

namespace TechWorldPMS.Pages.Customer
{
    [Authorize(Policy = "RequireCustomerOrAdmin")]
    public class CartModel : PageModel
    {
        private readonly ICartRepository _cartRepository;

        public CartModel(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public List<CartItem> CartItems { get; set; } = new();
        public decimal TotalAmount => CartItems.Sum(i => i.SubTotal);

        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();
            if (userId <= 0) return RedirectToPage("/Account/Login");

            CartItems = await _cartRepository.GetCartAsync(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync(int cartItemId, int quantity)
        {
            var userId = GetCurrentUserId();
            if (userId <= 0) return RedirectToPage("/Account/Login");

            await _cartRepository.UpdateQuantityAsync(cartItemId, userId, quantity);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveItemAsync(int cartItemId)
        {
            var userId = GetCurrentUserId();
            if (userId <= 0) return RedirectToPage("/Account/Login");

            await _cartRepository.RemoveItemAsync(cartItemId, userId);
            TempData["SuccessMessage"] = "Item removed from cart.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostClearCartAsync()
        {
            var userId = GetCurrentUserId();
            if (userId <= 0) return RedirectToPage("/Account/Login");

            await _cartRepository.ClearCartAsync(userId);
            TempData["SuccessMessage"] = "Cart cleared.";
            return RedirectToPage();
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out int id) ? id : 0;
        }
    }
}

