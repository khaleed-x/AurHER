using System.Text.Json.Serialization;

namespace AurHER.DTOs.Paystack
{
    public class InitializePaymentRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public int AmountInKobo { get; set; }  // Paystack uses kobo (1 Naira = 100 kobo)

        [JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;

        [JsonPropertyName("callback_url")]
        public string CallbackUrl { get; set; } = string.Empty;
    }
}