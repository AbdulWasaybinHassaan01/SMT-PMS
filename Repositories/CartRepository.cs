using System.Data;
using Microsoft.Data.SqlClient;
using TechWorldPMS.Data;
using TechWorldPMS.Models;

namespace TechWorldPMS.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CartRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<CartItem>> GetCartAsync(int userId)
        {
            var items = new List<CartItem>();
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                SELECT c.CartItemId, c.UserId, c.ProductId, c.Quantity,
                       p.Name AS ProductName, p.Price, p.ImageUrl, p.StockQuantity,
                       cat.CategoryName
                FROM CartItems c
                INNER JOIN Products p ON c.ProductId = p.ProductId
                INNER JOIN Categories cat ON p.CategoryId = cat.CategoryId
                WHERE c.UserId = @UserId
                ORDER BY c.CartItemId DESC";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new CartItem
                {
                    CartItemId = reader.GetInt32(reader.GetOrdinal("CartItemId")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                    ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? null : reader.GetString(reader.GetOrdinal("ImageUrl")),
                    AvailableStock = reader.GetInt32(reader.GetOrdinal("StockQuantity")),
                    CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? null : reader.GetString(reader.GetOrdinal("CategoryName"))
                });
            }

            return items;
        }

        public async Task<int> GetCartCountAsync(int userId)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = "SELECT COALESCE(SUM(Quantity), 0) FROM CartItems WHERE UserId = @UserId";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            object? result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<bool> AddToCartAsync(int userId, int productId, int quantity)
        {
            if (quantity <= 0) quantity = 1;

            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            // Check if item already exists in cart for this user
            string checkQuery = "SELECT CartItemId, Quantity FROM CartItems WHERE UserId = @UserId AND ProductId = @ProductId";
            using var checkCmd = new SqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@UserId", userId);
            checkCmd.Parameters.AddWithValue("@ProductId", productId);

            using var reader = await checkCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                int cartItemId = reader.GetInt32(0);
                int existingQty = reader.GetInt32(1);
                reader.Close();

                string updateQuery = "UPDATE CartItems SET Quantity = @Quantity WHERE CartItemId = @CartItemId";
                using var updateCmd = new SqlCommand(updateQuery, conn);
                updateCmd.Parameters.AddWithValue("@Quantity", existingQty + quantity);
                updateCmd.Parameters.AddWithValue("@CartItemId", cartItemId);
                return (await updateCmd.ExecuteNonQueryAsync()) > 0;
            }
            reader.Close();

            string insertQuery = @"
                INSERT INTO CartItems (UserId, ProductId, Quantity, CreatedAt)
                VALUES (@UserId, @ProductId, @Quantity, GETUTCDATE())";

            using var insertCmd = new SqlCommand(insertQuery, conn);
            insertCmd.Parameters.AddWithValue("@UserId", userId);
            insertCmd.Parameters.AddWithValue("@ProductId", productId);
            insertCmd.Parameters.AddWithValue("@Quantity", quantity);

            return (await insertCmd.ExecuteNonQueryAsync()) > 0;
        }

        public async Task<bool> UpdateQuantityAsync(int cartItemId, int userId, int quantity)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            if (quantity <= 0)
            {
                string deleteQuery = "DELETE FROM CartItems WHERE CartItemId = @CartItemId AND UserId = @UserId";
                using var delCmd = new SqlCommand(deleteQuery, conn);
                delCmd.Parameters.AddWithValue("@CartItemId", cartItemId);
                delCmd.Parameters.AddWithValue("@UserId", userId);
                return (await delCmd.ExecuteNonQueryAsync()) > 0;
            }
            else
            {
                string updateQuery = "UPDATE CartItems SET Quantity = @Quantity WHERE CartItemId = @CartItemId AND UserId = @UserId";
                using var updCmd = new SqlCommand(updateQuery, conn);
                updCmd.Parameters.AddWithValue("@Quantity", quantity);
                updCmd.Parameters.AddWithValue("@CartItemId", cartItemId);
                updCmd.Parameters.AddWithValue("@UserId", userId);
                return (await updCmd.ExecuteNonQueryAsync()) > 0;
            }
        }

        public async Task<bool> RemoveItemAsync(int cartItemId, int userId)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = "DELETE FROM CartItems WHERE CartItemId = @CartItemId AND UserId = @UserId";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@CartItemId", cartItemId);
            cmd.Parameters.AddWithValue("@UserId", userId);

            return (await cmd.ExecuteNonQueryAsync()) > 0;
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = "DELETE FROM CartItems WHERE UserId = @UserId";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            return (await cmd.ExecuteNonQueryAsync()) > 0;
        }
    }
}

