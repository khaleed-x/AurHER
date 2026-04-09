namespace AurHER.DTOs.Payment
{
    public class PaymentInitResult
    {
        public bool Success { get; set; }
        public string? AuthorizationUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class PaymentCallbackResult
    {
        public bool Success { get; set; }
        public bool PaymentSuccessful { get; set; }
        public int OrderId { get; set; }
        public string? ErrorMessage { get; set; }
    }

}