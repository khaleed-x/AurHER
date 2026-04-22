namespace AurHER.Models
{
    public class Product
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<ProductVariant> Variants { get; set; }

    public ICollection<ProductImage> Images { get; set; }
}
}