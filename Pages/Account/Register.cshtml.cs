using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechWorldPMS.Services;

namespace TechWorldPMS.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;

        public RegisterModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Full Name is required")]
            [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be at least 2 characters")]
            [Display(Name = "Full Name")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email address is required")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address")]
            [Display(Name = "Email Address")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Confirm Password is required")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Passwords do not match")]
            [Display(Name = "Confirm Password")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Display(Name = "SECRET-CODE:")]
            public string? SecretCode { get; set; }

            [Display(Name = "House / Street Address")]
            public string? Address { get; set; }

            [Display(Name = "City")]
            public string? City { get; set; }

            [Display(Name = "Phone Number")]
            public string? Phone { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var (success, message, user) = await _authService.RegisterAsync(
                Input.FullName,
                Input.Email,
                Input.Password,
                Input.ConfirmPassword,
                Input.SecretCode,
                Input.Address,
                Input.City,
                Input.Phone);

            if (!success || user == null)
            {
                ModelState.AddModelError(string.Empty, message);
                return Page();
            }

            // Sign user in automatically
            var principal = _authService.CreateClaimsPrincipal(user, user.Role);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (user.Role == "Admin" || user.Role == "Stock Manager")
            {
                return RedirectToPage("/Admin/Index");
            }
            else
            {
                return RedirectToPage("/Customer/Index");
            }
        }
    }
}

