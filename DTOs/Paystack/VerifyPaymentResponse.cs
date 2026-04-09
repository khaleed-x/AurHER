using System.Text.Json.Serialization;

namespace AurHER.DTOs.Paystack
{
    public class VerifyPaymentResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public VerifyPaymentData Data { get; set; } = new();
    }

    public class VerifyPaymentData
    {
        [JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;  // "success", "failed"

        [JsonPropertyName("amount")]
        public int AmountInKobo { get; set; }

        [JsonPropertyName("paid_at")]
        public DateTime? PaidAt { get; set; }

        [JsonPropertyName("channel")]
        public string Channel { get; set; } = string.Empty;

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;
    }
}