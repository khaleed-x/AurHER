namespace AurHER.DTOs.Store
{
    public class HomeProductDetailDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public List<HomeVariantDto> Variants { get; set; } = new();
        public List<HomeImageDto> Images { get; set; } = new();
    }
}