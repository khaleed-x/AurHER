using AurHER.Models.Enums;

namespace AurHER.DTOs.Admin
{
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public string ConfirmationCode { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ShippingAddress { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public PaymentInfoDto? Payment { get; set; }
        public List<OrderItemDetailDto> Items { get; set; } = new();
    }
}