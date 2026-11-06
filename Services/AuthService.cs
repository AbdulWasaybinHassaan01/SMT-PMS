using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using TechWorldPMS.Models;
using TechWorldPMS.Repositories;
using TechWorldPMS.Security;

namespace TechWorldPMS.Services
{
    public class AuthService : IAuthService
    {
        public const string AdminSecretCode = "ANYONEUSEIT";
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<(bool Success, string Message, User? User)> AuthenticateAsync(string email, string password, string? secretCode)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Please provide both email and password.", null);
            }

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || !PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                return (false, "Invalid email or password.", null);
            }

            // Check Secret Code
            bool hasValidSecret = !string.IsNullOrWhiteSpace(secretCode) && 
                                  secretCode.Trim().Equals(AdminSecretCode, StringComparison.OrdinalIgnoreCase);

            if (hasValidSecret)
            {
                // Elevated to Admin & Stock Manager
                user.Role = "Admin";
            }
            else
            {
                // If no secret code was provided or doesn't match ANYONEUSEIT:
                // If user is Admin in DB but didn't enter secret code, we can preserve role or default to customer as requested:
                // "where we enter ANYONEUSEIT and logged in as admin and stock manager. and if we didnt enter the code it will automatically logged in as customer."
                if (!string.Equals(user.Email, "admin@techworld.com", StringComparison.OrdinalIgnoreCase))
                {
                    user.Role = "Customer";
                }
            }

            return (true, "Authentication successful.", user);
        }

        public async Task<(bool Success, string Message, User? User)> RegisterAsync(
            string fullName, 
            string email, 
            string password, 
            string confirmPassword, 
            string? secretCode,
            string? address,
            string? city,
            string? phone)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return (false, "Full Name is required.", null);

            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                return (false, "A valid email address is required.", null);

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                return (false, "Password must be at least 6 characters long.", null);

            if (password != confirmPassword)
                return (false, "Passwords do not match.", null);

            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
                return (false, "An account with this email already exists.", null);

            bool isAdmin = !string.IsNullOrWhiteSpace(secretCode) && 
                           secretCode.Trim().Equals(AdminSecretCode, StringComparison.OrdinalIgnoreCase);

            string role = isAdmin ? "Admin" : "Customer";

            var newUser = new User
            {
                FullName = fullName.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = PasswordHasher.HashPassword(password),
                Role = role,
                Address = address?.Trim(),
                City = city?.Trim(),
                Phone = phone?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            int userId = await _userRepository.CreateAsync(newUser);
            if (userId <= 0)
                return (false, "Failed to create account. Please try again.", null);

            newUser.UserId = userId;
            return (true, "Account created successfully.", newUser);
        }

        public ClaimsPrincipal CreateClaimsPrincipal(User user, string activeRole)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, activeRole),
                new Claim("UserRoleDisplay", activeRole == "Admin" ? "Admin & Stock Manager" : "Customer")
            };

            if (activeRole == "Admin")
            {
                claims.Add(new Claim(ClaimTypes.Role, "Stock Manager"));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            return new ClaimsPrincipal(identity);
        }
    }
}

