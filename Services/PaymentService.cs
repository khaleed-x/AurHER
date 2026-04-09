using AurHER.Data;
using AurHER.DTOs.Payment;

using AurHER.Models;
using AurHER.Models.Enums;
using AurHER.Repositories.Interfaces;
using AurHER.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;


namespace AurHER.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaystackService _paystackService;
        private readonly PaystackSettings _paystackSettings;
        private readonly ILogger<PaymentService> _logger;
        private readonly INotificationService _notificationService;
        

        public PaymentService(
            AppDbContext context,
            IOrderRepository orderRepository,
            IPaymentRepository paymentRepository,
            IPaystackService paystackService,
            IOptions<PaystackSettings> paystackSettings,
            ILogger<PaymentService> logger,
             INotificationService notificationService )
        {
            _context = context;
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _paystackService = paystackService;
            _paystackSettings = paystackSettings.Value;
            _logger = logger;
              _notificationService = notificationService;
        }

        public async Task<PaymentInitResult> InitializePaymentAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return new PaymentInitResult { Success = false, ErrorMessage = "Order not found" };
            }

            var payment = await _paymentRepository.GetPaymentByOrderIdAsync(orderId);
            if (payment == null)
            {
                return new PaymentInitResult { Success = false, ErrorMessage = "Payment record not found" };
            }

            var reference = $"ORDER_{orderId}_{DateTime.UtcNow.Ticks}";
            payment.TransactionReference = reference;
            await _paymentRepository.UpdateAsync(payment);

            var response = await _paystackService.InitializePaymentAsync(
                order.Email,
                order.TotalAmount,
                reference,
                _paystackSettings.CallbackUrl
            );

            if (response == null || !response.Status)
            {
                _logger.LogError("Paystack initialization failed: {Message}", response?.Message);
                return new PaymentInitResult { Success = false, ErrorMessage = "Unable to initialize payment" };
            }

            return new PaymentInitResult
            {
                Success = true,
                AuthorizationUrl = response.Data.AuthorizationUrl
            };
        }

        public async Task<PaymentCallbackResult> HandleCallbackAsync(string transactionReference)
        {
            if (string.IsNullOrEmpty(transactionReference))
            {
                return new PaymentCallbackResult { Success = false, ErrorMessage = "Invalid payment reference" };
            }

            var verification = await _paystackService.VerifyPaymentAsync(transactionReference);

            if (verification == null || !verification.Status)
            {
                return new PaymentCallbackResult { Success = false, ErrorMessage = "Payment verification failed" };
            }

            var paymentData = verification.Data;
            var payment = await _paymentRepository.GetPaymentByTransactionReferenceAsync(transactionReference);
            
            if (payment == null)
            {
                return new PaymentCallbackResult { Success = false, ErrorMessage = "Payment record not found" };
            }

            if (payment.Status == PaymentStatus.Completed)
            {
                return new PaymentCallbackResult { Success = true, PaymentSuccessful = true, OrderId = payment.OrderId };
            }

            if (paymentData.Status.ToLower() == "success")
            {
                payment.Status = PaymentStatus.Completed;
                payment.PaidAt = paymentData.PaidAt;
                await _paymentRepository.UpdateAsync(payment);

                var order = await _orderRepository.GetByIdAsync(payment.OrderId);
                if (order != null && order.Status == OrderStatus.Pending)
                {
                    order.Status = OrderStatus.Processing;
                    await _orderRepository.UpdateAsync(order);

                    // Deduct stock using _context directly
                    await DeductStock(order.Id);
                }

                return new PaymentCallbackResult {
                     Success = true, 
                     PaymentSuccessful = true, 
                     OrderId = payment.OrderId };
            }
            if (paymentData.Status.ToLower() != "success")
            {
                payment.Status = PaymentStatus.Failed;
                await _paymentRepository.UpdateAsync(payment);

                await ReleaseReservedStockAsync(payment.OrderId);
                
                return new PaymentCallbackResult 
                { 
                    Success = true, 
                    PaymentSuccessful = false, 
                    OrderId = payment.OrderId 
                };
            }

            payment.Status = PaymentStatus.Failed;
            await _paymentRepository.UpdateAsync(payment);

            return new PaymentCallbackResult {
                 Success = true,
                  PaymentSuccessful = false, 
                  OrderId = payment.OrderId };
        }

        public async Task HandleWebhookAsync(string jsonBody, string signature)
        {
            // Verify signature
            if (!VerifyWebhookSignature(jsonBody, signature))
            {
                _logger.LogWarning("Invalid webhook signature");
                return;
            }

            // Parse JSON 
            using var jsonDoc = JsonDocument.Parse(jsonBody);
            var root = jsonDoc.RootElement;
            
            var eventType = root.GetProperty("event").GetString();
            if (eventType != "charge.success")
            {
                return;
            }

            var transactionRef = root.GetProperty("data").GetProperty("reference").GetString();
            if (string.IsNullOrEmpty(transactionRef))
            {
                return;
            }

            var payment = await _paymentRepository.GetPaymentByTransactionReferenceAsync(transactionRef);
            if (payment == null)
            {
                _logger.LogWarning("Webhook: Payment not found for reference {Reference}", transactionRef);
                return;
            }

            if (payment.Status == PaymentStatus.Completed)
            {
                return;
            }

            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment);

            var order = await _orderRepository.GetByIdAsync(payment.OrderId);
            if (order != null && order.Status == OrderStatus.Pending)
            {
                order.Status = OrderStatus.Processing;
                await _orderRepository.UpdateAsync(order);
                await DeductStock(order.Id);
            }


              if (eventType == "charge.failed")
                {
                    if (payment.Status == PaymentStatus.Failed)
                    {
                        return;
                    }

                    payment.Status = PaymentStatus.Failed;
                    await _paymentRepository.UpdateAsync(payment);
                    
                    _logger.LogInformation("Webhook: Payment failed for reference {Reference}", transactionRef);
                }
        }


        private async Task<bool> DeductStock(int orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            if (order?.Items == null) return false;

            foreach (var item in order.Items)
            {
                 bool success = false;
                int retryCount = 0;
                while (!success && retryCount < 3)
                {
                    try
                    {
                        var variant = await _context.ProductVariants
                            .FirstOrDefaultAsync(v => v.Id == item.ProductVariantId);
                        
                        if (variant == null) return false;
                        
                        // Convert reserved to actual sold
                        variant.ReservedStock -= item.Quantity;
                        variant.StockQuantity -= item.Quantity;
                       
                          //notify new order to admin nd customer
                        await _notificationService.NotifyNewOrderAsync(
                        order.Id, order.CustomerName,
                        order.TotalAmount, order.ConfirmationCode, order.Email);

                        // Notify if low or out of stock
                        var productName = variant.Product?.Name ?? "Unknown Product";
                        if (variant.StockQuantity == 0)
                            await _notificationService.NotifyOutOfStockAsync(
                            productName, variant.SKU);
                        else if (variant.StockQuantity <= 5)
                            await _notificationService.NotifyLowStockAsync(
                            productName, variant.SKU, variant.StockQuantity);

                        await _context.SaveChangesAsync();
                        success = true;
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        retryCount++;
                        if (retryCount >= 3) return false;
                        // Continue retry
                    }
                }

                 
                        // var variant = await _context.ProductVariants.FindAsync(item.ProductVariantId);
                        // if (variant != null && variant.StockQuantity >= item.Quantity)
                        // {
                        //     variant.StockQuantity -= item.Quantity;

                        //     //notify new order to admin nd customer
                        // await _notificationService.NotifyNewOrderAsync(
                        // order.Id, order.CustomerName,
                        // order.TotalAmount, order.ConfirmationCode, order.Email);

                        //     // Notify if low or out of stock
                        //        var productName = variant.Product?.Name ?? "Unknown Product";
                        //     if (variant.StockQuantity == 0)
                        //         await _notificationService.NotifyOutOfStockAsync(
                        //            productName, variant.SKU);
                        //     else if (variant.StockQuantity <= 5)
                        //         await _notificationService.NotifyLowStockAsync(
                        //             productName, variant.SKU, variant.StockQuantity);
           }
           return true;

                    //}
                    // await _context.SaveChangesAsync();
        }


        public async Task ReleaseReservedStockAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            if (order?.Items == null) return;
            
            foreach (var item in order.Items)
            {
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.Id == item.ProductVariantId);
                
                if (variant != null)
                {
                    variant.ReservedStock -= item.Quantity;
                }
            }
            
            await _context.SaveChangesAsync();
        }

        private bool VerifyWebhookSignature(string jsonBody, string signature)
        {
            if (string.IsNullOrEmpty(signature)) return false;

            var secretKey = _paystackSettings.SecretKey;
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(jsonBody));
            var computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            return computedSignature == signature.ToLower();
        }

            public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
            {
                return await _paymentRepository.GetPaymentByOrderIdAsync(orderId);
            }

            public async Task ResetPaymentForRetryAsync(int orderId)
            {
                var payment = await _paymentRepository.GetPaymentByOrderIdAsync(orderId);
                if (payment != null && payment.Status == PaymentStatus.Failed)
                {
                    payment.Status = PaymentStatus.Pending;
                    payment.TransactionReference = null;
                    await _paymentRepository.UpdateAsync(payment);
                }
            }
    }
}