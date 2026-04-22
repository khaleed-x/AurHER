namespace AurHER.DTOs.Store
{
    public class HomePageDto
    {
        public List<HomeCategoryDto> Categories { get; set; } = new();
        public List<HomeCollectionDto> Collections { get; set; } = new();
        public List<HomeProductDto> NewArrivals { get; set; } = new();
    }
}
