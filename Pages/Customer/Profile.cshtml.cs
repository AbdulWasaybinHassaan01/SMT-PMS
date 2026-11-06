using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechWorldPMS.Repositories;

namespace TechWorldPMS.Pages.Customer
{
    [Authorize(Policy = "RequireCustomerOrAdmin")]
    public class ProfileModel : PageModel
    {
        private readonly IUserRepository _userRepository;

        public ProfileModel(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [BindProperty]
        public ProfileInputModel Input { get; set; } = new();

        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        [TempData]
        public string? SuccessMessage { get; set; }

        public class ProfileInputModel
        {
            [Required(ErrorMessage = "Full Name is required")]
            [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
            [Display(Name = "Full Name")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Phone Number is required")]
            [Phone(ErrorMessage = "Please enter a valid phone number")]
            [Display(Name = "Phone Number")]
            public string Phone { get; set; } = string.Empty;

            [Required(ErrorMessage = "House / Residential Address is required")]
            [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters")]
            [Display(Name = "House / Street Address")]
            public string Address { get; set; } = string.Empty;

            [Required(ErrorMessage = "City is required")]
            [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
            [Display(Name = "City")]
            public string City { get; set; } = string.Empty;

            [StringLength(20, ErrorMessage = "Postal Code cannot exceed 20 characters")]
            [Display(Name = "Postal Code")]
            public string? PostalCode { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claim, out int userId))
            {
                return RedirectToPage("/Account/Login");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            Email = user.Email;
            Role = user.Role;

            Input = new ProfileInputModel
            {
                FullName = user.FullName,
                Phone = user.Phone ?? string.Empty,
                Address = user.Address ?? string.Empty,
                City = user.City ?? string.Empty,
                PostalCode = user.PostalCode ?? string.Empty
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claim, out int userId))
            {
                return RedirectToPage("/Account/Login");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            Email = user.Email;
            Role = user.Role;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            user.FullName = Input.FullName.Trim();
            user.Phone = Input.Phone.Trim();
            user.Address = Input.Address.Trim();
            user.City = Input.City.Trim();
            user.PostalCode = Input.PostalCode?.Trim();

            await _userRepository.UpdateProfileAsync(user);

            TempData["SuccessMessage"] = "Your house address and profile information have been updated successfully!";
            return RedirectToPage();
        }
    }
}

