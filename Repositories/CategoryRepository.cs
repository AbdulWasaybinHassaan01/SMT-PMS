using System.Data;
using Microsoft.Data.SqlClient;
using TechWorldPMS.Data;
using TechWorldPMS.Models;

namespace TechWorldPMS.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CategoryRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            var categories = new List<Category>();
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                SELECT c.CategoryId, c.CategoryName, c.Description, c.ColorBadge,
                       COUNT(p.ProductId) AS ProductCount
                FROM Categories c
                LEFT JOIN Products p ON c.CategoryId = p.CategoryId
                GROUP BY c.CategoryId, c.CategoryName, c.Description, c.ColorBadge
                ORDER BY c.CategoryName ASC";

            using var cmd = new SqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                categories.Add(new Category
                {
                    CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                    CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    ColorBadge = reader.GetString(reader.GetOrdinal("ColorBadge")),
                    ProductCount = reader.GetInt32(reader.GetOrdinal("ProductCount"))
                });
            }

            return categories;
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                SELECT c.CategoryId, c.CategoryName, c.Description, c.ColorBadge,
                       COUNT(p.ProductId) AS ProductCount
                FROM Categories c
                LEFT JOIN Products p ON c.CategoryId = p.CategoryId
                WHERE c.CategoryId = @Id
                GROUP BY c.CategoryId, c.CategoryName, c.Description, c.ColorBadge";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Category
                {
                    CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                    CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    ColorBadge = reader.GetString(reader.GetOrdinal("ColorBadge")),
                    ProductCount = reader.GetInt32(reader.GetOrdinal("ProductCount"))
                };
            }

            return null;
        }

        public async Task<int> AddAsync(Category category)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                INSERT INTO Categories (CategoryName, Description, ColorBadge)
                OUTPUT INSERTED.CategoryId
                VALUES (@CategoryName, @Description, @ColorBadge)";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
            cmd.Parameters.AddWithValue("@Description", (object?)category.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ColorBadge", category.ColorBadge);

            object? result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<bool> UpdateAsync(Category category)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                UPDATE Categories
                SET CategoryName = @CategoryName,
                    Description = @Description,
                    ColorBadge = @ColorBadge
                WHERE CategoryId = @CategoryId";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@CategoryId", category.CategoryId);
            cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
            cmd.Parameters.AddWithValue("@Description", (object?)category.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ColorBadge", category.ColorBadge);

            return (await cmd.ExecuteNonQueryAsync()) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = "DELETE FROM Categories WHERE CategoryId = @Id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            return (await cmd.ExecuteNonQueryAsync()) > 0;
        }
    }
}

