using System.ComponentModel.DataAnnotations;

namespace AurHER.Models
{
    public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public decimal Price { get; set; }
    public Product Product { get; set; }
    public string Size { get; set; }
    public string Color { get; set; }
    public int StockQuantity { get; set; }
    public string SKU { get; set; }
    public int ReservedStock { get; set; }  // Stock held for pending payments
    public int AvailableStock => StockQuantity - ReservedStock;
}
}