using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityTuitionApi.Data;
using UniversityTuitionApi.Models;
using UniversityTuitionApi.Services;

namespace UniversityTuitionApi.Controllers
{
    [ApiController]
    [Route("api/v1/Payments")]
    [Authorize] // 🔐 Ödeme almak da korumalı olsun
    public class PaymentsController : ControllerBase
    {
        private readonly UniversityContext _context;
        private readonly ITuitionService _tuitionService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            UniversityContext context,
            ITuitionService tuitionService,
            ILogger<PaymentsController> logger)
        {
            _context = context;
            _tuitionService = tuitionService;
            _logger = logger;
        }

        public class PaymentRequest
        {
            public string StudentNo { get; set; } = null!;
            public string Term { get; set; } = null!;
            public decimal Amount { get; set; }
        }

        // POST /api/v1/Payments
        // Body:
        // {
        //   "studentNo": "99999",
        //   "term": "2025-Fall",
        //   "amount": 2500
        // }
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.StudentNo) ||
                string.IsNullOrWhiteSpace(request.Term) ||
                request.Amount <= 0)
            {
                return BadRequest("studentNo, term ve pozitif amount zorunlu.");
            }

            var (success, message, remainingBalance) =
                await _tuitionService.ApplyPaymentAsync(
                    request.StudentNo,
                    request.Term,
                    request.Amount);

            if (!success)
            {
                _logger.LogWarning(
                    "Payment failed for {StudentNo} - {Term}: {Message}",
                    request.StudentNo, request.Term, message);

                return BadRequest(new
                {
                    status = "Error",
                    message
                });
            }

            _logger.LogInformation(
                "Payment succeeded for {StudentNo} - {Term}. Remaining balance: {Balance}",
                request.StudentNo, request.Term, remainingBalance);

            return Ok(new
            {
                status = "Successful",
                studentNo = request.StudentNo,
                term = request.Term,
                remainingBalance
            });
        }

        // GET /api/v1/Payments/{studentNo}?page=1&pageSize=10
        // Belirli öğrencinin tüm ödemelerini paging ile döner
        [HttpGet("{studentNo}")]
        public async Task<IActionResult> GetPaymentsForStudent(
            string studentNo,
            int page = 1,
            int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Payments
                .Where(p => p.StudentNo == studentNo)
                .OrderByDescending(p => p.PaidAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new
            {
                studentNo,
                page,
                pageSize,
                totalCount,
                items
            };

            return Ok(result);
        }
        // DELETE /api/v1/Payments/{paymentId}
        [HttpDelete("{paymentId}")]
        public async Task<IActionResult> DeletePayment(int paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);

            if (payment == null)
                return NotFound(new { message = "Ödeme kaydı bulunamadı." });

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ödeme kaydı silindi." });
        }

    }
}
