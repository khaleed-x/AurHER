namespace AurHER.DTOs.Store
{
    public class HomeCollectionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }   
        public List<HomeProductDto> Products { get; set; } = new();
    }
}