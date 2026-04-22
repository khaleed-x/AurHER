using Microsoft.AspNetCore.Mvc;
using AurHER.Services.Interfaces;
using AurHER.Models.Enums;

namespace AurHER.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;

        public PaymentController(IPaymentService paymentService,
        IOrderService orderService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
        }

      
        public async Task<IActionResult> Initialize(int orderId)
        {
            var result = await _paymentService.InitializePaymentAsync(orderId);
            
            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction("Index", "Checkout");
            }

            return Redirect(result.AuthorizationUrl);
        }

        [HttpGet] 
        public async Task<IActionResult> Callback(string reference, string trxref)
        {

            var transactionRef = reference ?? trxref;
            var result = await _paymentService.HandleCallbackAsync(transactionRef);

            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction("Index", "Checkout");
            }

            if (result.PaymentSuccessful)
            {
               var order = await _orderService.GetByIdAsync(result.OrderId);
                return RedirectToAction("Success", "Payment", new { code = order?.ConfirmationCode });
            }

            TempData["Error"] = "Payment failed. Please try again.";
            
         
             return RedirectToAction("Failed", "Payment", new { orderId = result.OrderId });
         }

            [HttpGet] 
            public IActionResult Success(string code)
            {
                if (string.IsNullOrEmpty(code))
                    return RedirectToAction("Index", "Home");

                ViewBag.ConfirmationCode = code;
                return View();
            }


            [HttpGet]
            public async Task<IActionResult> Failed(int orderId)
            {
            
                var order = await _orderService.GetByIdAsync(orderId);
                if (order == null)
                    return RedirectToAction("Index", "Home");
                
                ViewBag.OrderId = orderId;
                ViewBag.ConfirmationCode = order.ConfirmationCode;
                ViewBag.TotalAmount = order.TotalAmount;
             
                
                return View();
            }

            [HttpGet]
        public async Task<IActionResult> Retry(int orderId)
        {
            // Check if order still exists and payment failed
            var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
            if (payment == null || payment.Status != PaymentStatus.Failed)
            {
                return RedirectToAction("Index", "Cart");
            }
            
            // Reset payment status to Pending for retry
            payment.Status = PaymentStatus.Pending;
            payment.TransactionReference = null;
            await _paymentService.ResetPaymentForRetryAsync(orderId);
            
            // Redirect to initialize new payment
            return RedirectToAction("Initialize", new { orderId = orderId });
        }
   
        [HttpPost]
        public async Task<IActionResult> Webhook()
        {
            string jsonString;
            using (var reader = new StreamReader(Request.Body))
            {
                jsonString = await reader.ReadToEndAsync();
            }

            var signature = Request.Headers["x-paystack-signature"];
            await _paymentService.HandleWebhookAsync(jsonString, signature);
            
            return Ok();
        }

    }
}