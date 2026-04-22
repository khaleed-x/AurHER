using AurHER.Models;
using AurHER.Models.Enums;

namespace AurHER.Repositories.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<Payment?> GetPaymentByOrderIdAsync(int orderId);
        Task<Payment?> GetPaymentByTransactionReferenceAsync(string transactionReference);
        Task UpdatePaymentStatusAsync(int paymentId, PaymentStatus status, string transactionReference);
    }
}