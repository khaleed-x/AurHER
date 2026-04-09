using AurHER.Models.Enums;

namespace AurHER.DTOs.Admin
{
    public class RecentOrderDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string ConfirmationCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}