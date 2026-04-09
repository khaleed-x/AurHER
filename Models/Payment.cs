using AurHER.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AurHER.Models
{
    public class Payment
    {
        public int Id {get; set;}
        public int OrderId { get; set; }

        public Order Order { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; }

        [Column(TypeName = "text")]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    

        public string? TransactionReference { get; set; }

        public DateTime? PaidAt { get; set; }
    }
}