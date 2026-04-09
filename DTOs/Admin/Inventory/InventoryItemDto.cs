namespace AurHER.DTOs.Admin
{
    public class InventoryItemDto
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public int StockQuantity { get; set; }
        public string? CategoryName { get; set; }
        public string StockStatus { get; set; }
    }
}