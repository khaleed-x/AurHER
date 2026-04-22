using AurHER.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;


namespace AurHER.Models
{ 
    public class Order
    {
        public int Id { get; set; }

        public string CustomerName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string ShippingAddress { get; set; }
        
        public string DeliveryLocation { get; set; } 
              
        public decimal DeliveryFee { get; set; }
        public decimal TotalAmount { get; set; }
        

        [Column(TypeName = "text")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public string ConfirmationCode { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> Items { get; set; }

        public Payment? Payment { get; set; }
    }
}