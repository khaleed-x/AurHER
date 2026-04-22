using System.Text;
using System.Text.Json;
using AurHER.DTOs.Paystack;
using AurHER.Models;
using AurHER.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AurHER.Services
{
    public class PaystackService : IPaystackService
    {
        private readonly HttpClient _httpClient;
        private readonly PaystackSettings _settings;

        public PaystackService(HttpClient httpClient, IOptions<PaystackSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<InitializePaymentResponse?> InitializePaymentAsync(string email, decimal amount, string reference, string callbackUrl)
        {
            var request = new InitializePaymentRequest
            {
                Email = email,
                AmountInKobo = (int)(amount * 100),  // Convert Naira to Kobo
                Reference = reference,
                CallbackUrl = callbackUrl
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.SecretKey}");

            var response = await _httpClient.PostAsync(_settings.InitializeUrl, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<InitializePaymentResponse>(responseJson);
        }

        public async Task<VerifyPaymentResponse?> VerifyPaymentAsync(string reference)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.SecretKey}");

            var response = await _httpClient.GetAsync($"{_settings.VerifyUrl}{reference}");
            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<VerifyPaymentResponse>(responseJson);
        }
    }
}