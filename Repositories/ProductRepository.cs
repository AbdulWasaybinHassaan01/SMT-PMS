using System.Data;
using Microsoft.Data.SqlClient;
using TechWorldPMS.Data;
using TechWorldPMS.Models;

namespace TechWorldPMS.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ProductRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Product>> GetAllAsync(string? searchTerm = null, int? categoryId = null)
        {
            var products = new List<Product>();
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                SELECT p.ProductId, p.Name, p.CategoryId, c.CategoryName, p.Price, 
                       p.StockQuantity, p.Description, p.ImageUrl, p.CreatedAt, p.UpdatedAt
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE (1 = 1)";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query += " AND (p.Name LIKE @SearchTerm OR p.Description LIKE @SearchTerm)";
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query += " AND p.CategoryId = @CategoryId";
            }

            query += " ORDER BY p.ProductId DESC";

            using var cmd = new SqlCommand(query, conn);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm.Trim()}%");
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                cmd.Parameters.AddWithValue("@CategoryId", categoryId.Value);
            }

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(MapProduct(reader));
            }

            return products;
        }

        public async Task<IEnumerable<Product>> GetRecentAsync(int count = 5)
        {
            var products = new List<Product>();
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                SELECT TOP (@Count) p.ProductId, p.Name, p.CategoryId, c.CategoryName, p.Price, 
                       p.StockQuantity, p.Description, p.ImageUrl, p.CreatedAt, p.UpdatedAt
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                ORDER BY p.ProductId ASC";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Count", count);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(MapProduct(reader));
            }

            return products;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                SELECT p.ProductId, p.Name, p.CategoryId, c.CategoryName, p.Price, 
                       p.StockQuantity, p.Description, p.ImageUrl, p.CreatedAt, p.UpdatedAt
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE p.ProductId = @Id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapProduct(reader);
            }

            return null;
        }

        public async Task<int> AddAsync(Product product)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                INSERT INTO Products (Name, CategoryId, Price, StockQuantity, Description, ImageUrl, CreatedAt)
                OUTPUT INSERTED.ProductId
                VALUES (@Name, @CategoryId, @Price, @StockQuantity, @Description, @ImageUrl, GETUTCDATE())";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Name", product.Name);
            cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId);
            cmd.Parameters.AddWithValue("@Price", product.Price);
            cmd.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
            cmd.Parameters.AddWithValue("@Description", (object?)product.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ImageUrl", (object?)product.ImageUrl ?? DBNull.Value);

            object? result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                UPDATE Products
                SET Name = @Name,
                    CategoryId = @CategoryId,
                    Price = @Price,
                    StockQuantity = @StockQuantity,
                    Description = @Description,
                    ImageUrl = COALESCE(@ImageUrl, ImageUrl),
                    UpdatedAt = GETUTCDATE()
                WHERE ProductId = @ProductId";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ProductId", product.ProductId);
            cmd.Parameters.AddWithValue("@Name", product.Name);
            cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId);
            cmd.Parameters.AddWithValue("@Price", product.Price);
            cmd.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
            cmd.Parameters.AddWithValue("@Description", (object?)product.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ImageUrl", (object?)product.ImageUrl ?? DBNull.Value);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = "DELETE FROM Products WHERE ProductId = @Id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            var stats = new DashboardStats();
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                SELECT 
                    (SELECT COUNT(*) FROM Products) AS TotalProducts,
                    (SELECT COUNT(*) FROM Categories) AS TotalCategories,
                    (SELECT COUNT(*) FROM Products WHERE StockQuantity > 0) AS InStockCount,
                    (SELECT COUNT(*) FROM Products WHERE StockQuantity <= 5) AS LowStockCount";

            using var cmd = new SqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                stats.TotalProducts = reader.GetInt32(reader.GetOrdinal("TotalProducts"));
                stats.TotalCategories = reader.GetInt32(reader.GetOrdinal("TotalCategories"));
                stats.InStockCount = reader.GetInt32(reader.GetOrdinal("InStockCount"));
                stats.LowStockCount = reader.GetInt32(reader.GetOrdinal("LowStockCount"));
            }

            return stats;
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantityChange)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                UPDATE Products 
                SET StockQuantity = CASE 
                    WHEN StockQuantity + @QuantityChange < 0 THEN 0 
                    ELSE StockQuantity + @QuantityChange 
                END
                WHERE ProductId = @ProductId";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ProductId", productId);
            cmd.Parameters.AddWithValue("@QuantityChange", quantityChange);

            return (await cmd.ExecuteNonQueryAsync()) > 0;
        }

        private static Product MapProduct(SqlDataReader reader)
        {
            return new Product
            {
                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? null : reader.GetString(reader.GetOrdinal("CategoryName")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                StockQuantity = reader.GetInt32(reader.GetOrdinal("StockQuantity")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? null : reader.GetString(reader.GetOrdinal("ImageUrl")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }
    }
}

