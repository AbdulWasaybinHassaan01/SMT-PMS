namespace TechWorldPMS.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
        public string? ImageUrl { get; set; }
        public int AvailableStock { get; set; }
        public string? CategoryName { get; set; }

        public decimal SubTotal => Price * Quantity;
    }
}

