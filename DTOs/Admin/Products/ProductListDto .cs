namespace AurHER.DTOs.Admin
{
    public class ProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? CategoryName { get; set; }
        public bool IsActive { get; set; }
        public int VariantCount { get; set; }
        public decimal? LowestPrice { get; set; }
        public decimal? HighestPrice { get; set; }
        public string? PrimaryImage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}