namespace AurHER.DTOs.Admin
{
    public class ProductVariantDto
    {
        public int Id { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string SKU { get; set; }
        public int ReservedStock { get; set; }  // Stock held for pending payments
        public int AvailableStock => StockQuantity - ReservedStock;

    }
}