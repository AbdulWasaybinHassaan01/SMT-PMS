using TechWorldPMS.Models;

namespace TechWorldPMS.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task<int> CreateAsync(User user);
        Task<bool> UpdateProfileAsync(User user);
    }
}

