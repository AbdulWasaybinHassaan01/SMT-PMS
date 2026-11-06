using System.Data;
using Microsoft.Data.SqlClient;
using TechWorldPMS.Data;
using TechWorldPMS.Models;

namespace TechWorldPMS.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                SELECT UserId, FullName, Email, PasswordHash, Role, Address, City, PostalCode, Phone, CreatedAt
                FROM Users
                WHERE LOWER(Email) = LOWER(@Email)";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email.Trim());

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUser(reader);
            }

            return null;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                SELECT UserId, FullName, Email, PasswordHash, Role, Address, City, PostalCode, Phone, CreatedAt
                FROM Users
                WHERE UserId = @Id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUser(reader);
            }

            return null;
        }

        public async Task<int> CreateAsync(User user)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                INSERT INTO Users (FullName, Email, PasswordHash, Role, Address, City, PostalCode, Phone, CreatedAt)
                OUTPUT INSERTED.UserId
                VALUES (@FullName, @Email, @PasswordHash, @Role, @Address, @City, @PostalCode, @Phone, GETUTCDATE())";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@FullName", user.FullName);
            cmd.Parameters.AddWithValue("@Email", user.Email.Trim().ToLowerInvariant());
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            cmd.Parameters.AddWithValue("@Role", user.Role);
            cmd.Parameters.AddWithValue("@Address", (object?)user.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@City", (object?)user.City ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PostalCode", (object?)user.PostalCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object?)user.Phone ?? DBNull.Value);

            object? result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<bool> UpdateProfileAsync(User user)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                UPDATE Users
                SET FullName = @FullName,
                    Address = @Address,
                    City = @City,
                    PostalCode = @PostalCode,
                    Phone = @Phone
                WHERE UserId = @UserId";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", user.UserId);
            cmd.Parameters.AddWithValue("@FullName", user.FullName);
            cmd.Parameters.AddWithValue("@Address", (object?)user.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@City", (object?)user.City ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PostalCode", (object?)user.PostalCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object?)user.Phone ?? DBNull.Value);

            return (await cmd.ExecuteNonQueryAsync()) > 0;
        }

        private static User MapUser(SqlDataReader reader)
        {
            return new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                City = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString(reader.GetOrdinal("City")),
                PostalCode = reader.IsDBNull(reader.GetOrdinal("PostalCode")) ? null : reader.GetString(reader.GetOrdinal("PostalCode")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}

