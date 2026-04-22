namespace AurHER.Models
{
    public class Cart
{
    public int Id { get; set; }

    public string SessionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<CartItem> Items { get; set; }
}
}