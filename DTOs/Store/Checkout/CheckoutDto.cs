using AurHER.Models;
using System.ComponentModel.DataAnnotations;


namespace AurHER.DTOs.Store
{
    
    public class CheckoutDto
    {
        [Required]
        public string CustomerName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; }

        [Required]
        [Phone(ErrorMessage = "Enter a valid phone number")]
        public string Phone { get; set; }

        [Required]    
        public string DeliveryLocation { get; set; }
        public decimal DeliveryFee { get; set; }

        [Required]    
        public string ShippingAddress { get; set; }

      
    }

    public class CheckoutSummaryDto
    {
        public CartDto Cart { get; set; }
        public CheckoutDto Form { get; set; } = new();
        public IEnumerable<DeliveryLocation> DeliveryLocations { get; set; }
    }
}