namespace AurHER.DTOs.Admin
{
    public class CollectionDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<CollectionProductItemDto> Products { get; set; } = new();
        public List<AvailableProductDto> AvailableProducts { get; set; } = new();
    }

    public class CollectionProductItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string? PrimaryImage { get; set; }
        public int VariantCount { get; set; }
        public string? CategoryName { get; set; }
    }

    public class AvailableProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? CategoryName { get; set; }
    }
}