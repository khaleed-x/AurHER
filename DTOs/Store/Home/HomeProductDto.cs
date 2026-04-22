namespace AurHER.DTOs.Store
{
    public class HomeProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? PrimaryImage { get; set; }
        public decimal? LowestPrice { get; set; }
        public decimal? HighestPrice { get; set; }
        public string? CategoryName { get; set; }
        public bool HasVariants { get; set; }
    }
}