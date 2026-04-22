namespace AurHER.Models
{
    public class PaystackSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string InitializeUrl { get; set; } = "https://api.paystack.co/transaction/initialize";
        public string VerifyUrl { get; set; } = "https://api.paystack.co/transaction/verify/";
        public string CallbackUrl { get; set; } = string.Empty;
    }
}


