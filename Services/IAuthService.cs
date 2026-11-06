using System.Security.Claims;
using TechWorldPMS.Models;

namespace TechWorldPMS.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, User? User)> AuthenticateAsync(string email, string password, string? secretCode);
        Task<(bool Success, string Message, User? User)> RegisterAsync(string fullName, string email, string password, string confirmPassword, string? secretCode, string? address, string? city, string? phone);
        ClaimsPrincipal CreateClaimsPrincipal(User user, string activeRole);
    }
}

