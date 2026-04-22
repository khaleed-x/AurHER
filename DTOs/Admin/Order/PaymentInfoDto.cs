using AurHER.Models.Enums;

namespace AurHER.DTOs.Admin
{
    public class PaymentInfoDto
    {
        public PaymentStatus Status { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string? TransactionReference { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}