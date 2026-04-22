namespace AurHER.Models
{
    public class CollectionProduct
{
    public int Id {get; set;}
    public int CollectionId { get; set; }

    public Collection Collection { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; }
}
}