using System.ComponentModel.DataAnnotations;

namespace TechWorldPMS.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category Name is required")]
        [StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters")]
        public string? Description { get; set; }

        public string ColorBadge { get; set; } = "primary";

        public int ProductCount { get; set; }
    }
}

