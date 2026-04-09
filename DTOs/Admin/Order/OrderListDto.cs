using AurHER.Models.Enums;

namespace AurHER.DTOs.Admin
{
    public class OrderListDto
    {
        public int Id { get; set; }
        public string ConfirmationCode { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }  

    }
    public class CountDto
     {
            public int TotalOrderCount { get; set; }
            public int OrderToday { get; set; }
                
      }

    public class OrderDashboardDto
  {
        public CountDto Counts { get; set; }
        public IEnumerable<OrderListDto> Orders { get; set; }
  }

}