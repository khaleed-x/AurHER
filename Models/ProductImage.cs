namespace AurHER.Models
{
public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public String ImageUrl { get; set;}
        public bool Isprimary { get; set;}

    }
}