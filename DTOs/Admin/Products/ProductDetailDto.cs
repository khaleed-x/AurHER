namespace AurHER.DTOs.Admin
{
    public class ProductDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ProductVariantDto> Variants { get; set; } = new();
        public List<ProductImageDto> Images { get; set; } = new();
    }
}