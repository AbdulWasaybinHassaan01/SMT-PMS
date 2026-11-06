using System.Data;
using Microsoft.Data.SqlClient;
using TechWorldPMS.Data;
using TechWorldPMS.Models;

namespace TechWorldPMS.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public OrderRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<int> CreateOrderAsync(Order order, List<CartItem> items)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();
            using var trans = conn.BeginTransaction();

            try
            {
                // 1. Insert Order
                string insertOrderSql = @"
                    INSERT INTO Orders (UserId, CustomerName, CustomerEmail, OrderDate, TotalAmount, Status, 
                                        PaymentMethod, ShippingAddress, City, PostalCode, Phone, Notes)
                    OUTPUT INSERTED.OrderId
                    VALUES (@UserId, @CustomerName, @CustomerEmail, GETUTCDATE(), @TotalAmount, @Status,
                            @PaymentMethod, @ShippingAddress, @City, @PostalCode, @Phone, @Notes)";

                int orderId;
                using (var cmdOrder = new SqlCommand(insertOrderSql, conn, trans))
                {
                    cmdOrder.Parameters.AddWithValue("@UserId", order.UserId);
                    cmdOrder.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                    cmdOrder.Parameters.AddWithValue("@CustomerEmail", order.CustomerEmail);
                    cmdOrder.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                    cmdOrder.Parameters.AddWithValue("@Status", order.Status);
                    cmdOrder.Parameters.AddWithValue("@PaymentMethod", order.PaymentMethod);
                    cmdOrder.Parameters.AddWithValue("@ShippingAddress", order.ShippingAddress);
                    cmdOrder.Parameters.AddWithValue("@City", order.City);
                    cmdOrder.Parameters.AddWithValue("@PostalCode", (object?)order.PostalCode ?? DBNull.Value);
                    cmdOrder.Parameters.AddWithValue("@Phone", order.Phone);
                    cmdOrder.Parameters.AddWithValue("@Notes", (object?)order.Notes ?? DBNull.Value);

                    orderId = Convert.ToInt32(await cmdOrder.ExecuteScalarAsync());
                }

                // 2. Insert Order Items and Decrement Product Stock
                foreach (var item in items)
                {
                    string insertItemSql = @"
                        INSERT INTO OrderItems (OrderId, ProductId, ProductName, UnitPrice, Quantity, SubTotal, ImageUrl)
                        VALUES (@OrderId, @ProductId, @ProductName, @UnitPrice, @Quantity, @SubTotal, @ImageUrl)";

                    using (var cmdItem = new SqlCommand(insertItemSql, conn, trans))
                    {
                        cmdItem.Parameters.AddWithValue("@OrderId", orderId);
                        cmdItem.Parameters.AddWithValue("@ProductId", item.ProductId);
                        cmdItem.Parameters.AddWithValue("@ProductName", item.ProductName);
                        cmdItem.Parameters.AddWithValue("@UnitPrice", item.Price);
                        cmdItem.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmdItem.Parameters.AddWithValue("@SubTotal", item.SubTotal);
                        cmdItem.Parameters.AddWithValue("@ImageUrl", (object?)item.ImageUrl ?? DBNull.Value);

                        await cmdItem.ExecuteNonQueryAsync();
                    }

                    // Decrement stock in Products table
                    string updateStockSql = @"
                        UPDATE Products 
                        SET StockQuantity = CASE 
                            WHEN StockQuantity - @Quantity < 0 THEN 0 
                            ELSE StockQuantity - @Quantity 
                        END
                        WHERE ProductId = @ProductId";

                    using (var cmdStock = new SqlCommand(updateStockSql, conn, trans))
                    {
                        cmdStock.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmdStock.Parameters.AddWithValue("@ProductId", item.ProductId);
                        await cmdStock.ExecuteNonQueryAsync();
                    }
                }

                // 3. Clear user's cart
                string clearCartSql = "DELETE FROM CartItems WHERE UserId = @UserId";
                using (var cmdCart = new SqlCommand(clearCartSql, conn, trans))
                {
                    cmdCart.Parameters.AddWithValue("@UserId", order.UserId);
                    await cmdCart.ExecuteNonQueryAsync();
                }

                // Commit transaction
                await trans.CommitAsync();
                return orderId;
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = new List<Order>();
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                SELECT OrderId, UserId, CustomerName, CustomerEmail, OrderDate, TotalAmount, Status, 
                       PaymentMethod, ShippingAddress, City, PostalCode, Phone, Notes
                FROM Orders
                WHERE UserId = @UserId
                ORDER BY OrderId DESC";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                orders.Add(MapOrder(reader));
            }
            reader.Close();

            // Load items for each order
            foreach (var order in orders)
            {
                order.Items = await GetOrderItemsAsync(order.OrderId, conn);
            }

            return orders;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = new List<Order>();
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                SELECT OrderId, UserId, CustomerName, CustomerEmail, OrderDate, TotalAmount, Status, 
                       PaymentMethod, ShippingAddress, City, PostalCode, Phone, Notes
                FROM Orders
                ORDER BY OrderId DESC";

            using var cmd = new SqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                orders.Add(MapOrder(reader));
            }
            reader.Close();

            foreach (var order in orders)
            {
                order.Items = await GetOrderItemsAsync(order.OrderId, conn);
            }

            return orders;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = @"
                SELECT OrderId, UserId, CustomerName, CustomerEmail, OrderDate, TotalAmount, Status, 
                       PaymentMethod, ShippingAddress, City, PostalCode, Phone, Notes
                FROM Orders
                WHERE OrderId = @OrderId";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OrderId", orderId);

            Order? order = null;
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                order = MapOrder(reader);
            }
            reader.Close();

            if (order != null)
            {
                order.Items = await GetOrderItemsAsync(order.OrderId, conn);
            }

            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string query = "UPDATE Orders SET Status = @Status WHERE OrderId = @OrderId";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@OrderId", orderId);

            return (await cmd.ExecuteNonQueryAsync()) > 0;
        }

        private static async Task<List<OrderItem>> GetOrderItemsAsync(int orderId, SqlConnection conn)
        {
            var items = new List<OrderItem>();
            string query = @"
                SELECT OrderItemId, OrderId, ProductId, ProductName, UnitPrice, Quantity, SubTotal, ImageUrl
                FROM OrderItems
                WHERE OrderId = @OrderId";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OrderId", orderId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new OrderItem
                {
                    OrderItemId = reader.GetInt32(reader.GetOrdinal("OrderItemId")),
                    OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                    ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                    UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                    SubTotal = reader.GetDecimal(reader.GetOrdinal("SubTotal")),
                    ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? null : reader.GetString(reader.GetOrdinal("ImageUrl"))
                });
            }

            return items;
        }

        private static Order MapOrder(SqlDataReader reader)
        {
            return new Order
            {
                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                CustomerEmail = reader.GetString(reader.GetOrdinal("CustomerEmail")),
                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                ShippingAddress = reader.GetString(reader.GetOrdinal("ShippingAddress")),
                City = reader.GetString(reader.GetOrdinal("City")),
                PostalCode = reader.IsDBNull(reader.GetOrdinal("PostalCode")) ? null : reader.GetString(reader.GetOrdinal("PostalCode")),
                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes"))
            };
        }
    }
}

