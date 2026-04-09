using AurHER.Models.Enums;

namespace AurHER.DTOs.Store
{
    public class OrderTrackingDto
    {
        public string ConfirmationCode { get; set; }
        public string CustomerName { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderTrackingItemDto> Items { get; set; } = new();
        public PaymentStatus PaymentStatus { get; set; }
    }

    public class OrderTrackingItemDto
    {
        public string ProductName { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? PrimaryImage { get; set; }
    }
}