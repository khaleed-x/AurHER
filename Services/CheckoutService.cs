using AurHER.Data;
using AurHER.DTOs.Store;
using AurHER.Models;
using AurHER.Models.Enums;
using AurHER.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AurHER.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly ICartService _cartService;
        private readonly INotificationService _notificationService;
        private readonly AppDbContext _context;

        public CheckoutService(
            ICartService cartService,
            INotificationService notificationService,
            AppDbContext context)
        {
            _cartService = cartService;
            _notificationService = notificationService;
            _context = context;
        }

        public async Task<(int? OrderId, string ErrorMessage)> PlaceOrderAsync(string sessionId, CheckoutDto dto)
        {
            
            var cart = await _cartService.GetCartAsync(sessionId);
            if (!cart.Items.Any()) 
                return (null, "");

            foreach (var item in cart.Items)
            {
                var variant = await _context.ProductVariants.FindAsync(item.VariantId);
                if (variant == null)
                {
                    return (null, $"Product variant not found.");
                }

                if (variant.AvailableStock < item.Quantity)
                {
                    var productName = variant.Product?.Name ?? "Product";
                    return (null, $"Sorry, '{productName}' only has {variant.StockQuantity} left in stock. Please reduce quantity.");
                    
                }
                variant.ReservedStock += item.Quantity;
                  await _context.SaveChangesAsync();    
            }


            var confirmationCode = Guid.NewGuid().ToString("N")
                .Substring(0, 8).ToUpper();

      
        var deliveryLocation = await _context.DeliveryLocations
            .FirstOrDefaultAsync(l => l.Name == dto.DeliveryLocation);

        if (deliveryLocation == null)
        {
            return (null, "Invalid delivery location selected.");
        }

        var deliveryFee = deliveryLocation.Fee;

        var subtotal = cart.Items.Sum(i => i.Price * i.Quantity);

        var total = subtotal + deliveryFee;
        var fullAddress = $"{dto.ShippingAddress}, {deliveryLocation.Name}";
        
        var order = new Order
        {
            CustomerName = dto.CustomerName,
            Email = dto.Email,
            Phone = dto.Phone,
            ShippingAddress = fullAddress,
             DeliveryLocation = dto.DeliveryLocation,   
            DeliveryFee = deliveryFee,
            TotalAmount = total,
            Status = OrderStatus.Pending,
            ConfirmationCode = confirmationCode,
            CreatedAt = DateTime.UtcNow
        };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cart.Items)
            { 

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductVariantId = item.VariantId,
                    Quantity = item.Quantity,
                    Price = item.Price
                };
                _context.OrderItems.Add(orderItem);

            }

            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = total,
                PaymentMethod = "Paystack",
                Status = PaymentStatus.Pending
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            
            await _cartService.ClearCartAsync(sessionId);

          return (order.Id, null);
        }
    }
}