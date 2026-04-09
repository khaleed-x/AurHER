namespace AurHER.Models
{
    public class Collection
{
    public int Id { get; set; }

    public string Name { get; set; }

    public ICollection<CollectionProduct> Products { get; set; }
}
}