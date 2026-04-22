using System.ComponentModel.DataAnnotations;

namespace AurHER.DTOs.Admin
{
    public class UpdateVariantDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Size is required")]
        public string Size { get; set; }

        [Required(ErrorMessage = "Color is required")]
        public string Color { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "SKU is required")]
        public string SKU { get; set; }
    }
}