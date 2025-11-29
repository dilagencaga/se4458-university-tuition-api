using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityTuitionApi.Data;
using UniversityTuitionApi.Models;

namespace UniversityTuitionApi.Controllers
{
    [ApiController]
    [Route("api/v1/Payments")]
    [Authorize] // 🔐 Ödeme almak da korumalı olsun
    public class PaymentsController : ControllerBase
    {
        private readonly UniversityContext _context;

        public PaymentsController(UniversityContext context)
        {
            _context = context;
        }

        // POST /api/v1/Payments
        // Body:
        // {
        //   "studentNo": "99999",
        //   "term": "2025-Fall",
        //   "amount": 2500
        // }
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] Payment request)
        {
            if (string.IsNullOrWhiteSpace(request.StudentNo) ||
                string.IsNullOrWhiteSpace(request.Term) ||
                request.Amount <= 0)
            {
                return BadRequest("studentNo, term ve pozitif amount zorunlu.");
            }

            var tuition = await _context.TuitionRecords
                .FirstOrDefaultAsync(t =>
                    t.StudentNo == request.StudentNo &&
                    t.Term == request.Term);

            if (tuition == null)
            {
                return NotFound("Bu öğrenci ve dönem için tuition kaydı yok.");
            }

            if (request.Amount > tuition.Balance)
            {
                // İstersen burada kısmi ödeme yapma diyebilirsin, ben uyarı verip izin veriyorum
                request.Amount = tuition.Balance;
            }

            var payment = new Payment
            {
                StudentNo = request.StudentNo,
                Term = request.Term,
                Amount = request.Amount,
                PaidAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            tuition.Balance -= request.Amount;
            if (tuition.Balance < 0) tuition.Balance = 0;

            await _context.SaveChangesAsync();

            var response = new
            {
                status = "Successful",
                studentNo = tuition.StudentNo,
                term = tuition.Term,
                tuitionTotal = tuition.TuitionTotal,
                remainingBalance = tuition.Balance,
                paymentId = payment.Id
            };

            return Ok(response);
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
    }
}
