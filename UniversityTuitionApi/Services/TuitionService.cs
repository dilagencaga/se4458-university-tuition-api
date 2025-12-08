using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UniversityTuitionApi.Data;
using UniversityTuitionApi.Models;

namespace UniversityTuitionApi.Services
{
    public class TuitionService : ITuitionService
    {
        private readonly UniversityContext _context;
        private readonly ILogger<TuitionService> _logger;

        public TuitionService(UniversityContext context, ILogger<TuitionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TuitionRecord?> GetTuitionAsync(string studentNo, string term)
        {
            return await _context.TuitionRecords
                .FirstOrDefaultAsync(t => t.StudentNo == studentNo && t.Term == term);
        }

        public async Task<TuitionRecord> AddOrUpdateTuitionAsync(
            string studentNo,
            string term,
            decimal tuitionTotal)
        {
            var record = await GetTuitionAsync(studentNo, term);

            if (record == null)
            {
                record = new TuitionRecord
                {
                    StudentNo = studentNo,
                    Term = term,
                    TuitionTotal = tuitionTotal,
                    Balance = tuitionTotal
                };

                _context.TuitionRecords.Add(record);
            }
            else
            {
                record.TuitionTotal = tuitionTotal;
                record.Balance = tuitionTotal;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Tuition upserted for {StudentNo} - {Term}. Total: {Total}, Balance: {Balance}",
                studentNo, term, record.TuitionTotal, record.Balance);

            return record;
        }

        public async Task<(bool Success, string Message, decimal? RemainingBalance)> ApplyPaymentAsync(
            string studentNo,
            string term,
            decimal amount)
        {
            if (amount <= 0)
            {
                return (false, "Ödeme tutarı pozitif olmalı.", null);
            }

            var tuition = await GetTuitionAsync(studentNo, term);

            if (tuition == null)
            {
                _logger.LogWarning("Payment failed. Tuition not found for {StudentNo} - {Term}",
                    studentNo, term);
                return (false, "Bu öğrenci ve dönem için tuition kaydı yok.", null);
            }

            if (tuition.Balance <= 0)
            {
                return (false, "Bu dönem için ödenecek bakiye kalmamış.", tuition.Balance);
            }

            if (amount > tuition.Balance)
            {
                amount = tuition.Balance;
            }

            var payment = new Payment
            {
                StudentNo = studentNo,
                Term = term,
                Amount = amount,
                PaidAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            tuition.Balance -= amount;
            if (tuition.Balance < 0) tuition.Balance = 0;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Payment {Amount} applied to {StudentNo} - {Term}. Remaining balance: {Balance}",
                amount, studentNo, term, tuition.Balance);

            return (true, "Ödeme başarıyla uygulandı.", tuition.Balance);
        }
    }
}
