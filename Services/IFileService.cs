namespace TechWorldPMS.Services
{
    public interface IFileService
    {
        Task<string?> SaveProductImageAsync(IFormFile? file, string? fallbackUrl = null);
        void DeleteImage(string? relativePath);
    }
}

