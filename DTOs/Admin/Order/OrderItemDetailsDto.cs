namespace AurHER.DTOs.Admin
{
    public class OrderItemDetailDto
    {
        public string ProductName { get; set; }
        public string Variant { get; set; }  // e.g., "M / Red"
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
        public string? ProductImage { get; set; }
    }
}