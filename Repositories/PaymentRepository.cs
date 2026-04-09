using Microsoft.EntityFrameworkCore;
using AurHER.Data;
using AurHER.Models;
using AurHER.Models.Enums;
using AurHER.Repositories.Interfaces;

namespace AurHER.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        // Get all payments, no relations
        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments.ToListAsync();
        }

        // Get single payment by ID
        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments.FindAsync(id);
        }

        // Get payment linked to a specific order
        public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        // Get payment by Paystack transaction reference
        public async Task<Payment?> GetPaymentByTransactionReferenceAsync(string transactionReference)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.TransactionReference == transactionReference);
        }

        // Update payment status after Paystack webhook confirms payment
        public async Task UpdatePaymentStatusAsync(int paymentId, PaymentStatus status, string transactionReference)
        {
            var payment = await GetByIdAsync(paymentId);
            if (payment != null)
            {
                payment.Status = status;
                payment.TransactionReference = transactionReference;
                payment.PaidAt = DateTime.UtcNow;
                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var payment = await GetByIdAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }
        }
    }
}