using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityTuitionApi.Data;
using UniversityTuitionApi.Models;

namespace UniversityTuitionApi.Controllers
{
    [ApiController]
    [Route("api/v1/Admin")]
    [Authorize] // 🔐 Admin tarafı tamamen korumalı
    public class AdminController : ControllerBase
    {
        private readonly UniversityContext _context;

        public AdminController(UniversityContext context)
        {
            _context = context;
        }

        // POST /api/v1/Admin/tuition
        [HttpPost("tuition")]
        public async Task<ActionResult<TuitionRecord>> CreateOrUpdateTuition([FromBody] TuitionRecord request)
        {
            if (string.IsNullOrWhiteSpace(request.StudentNo) ||
                string.IsNullOrWhiteSpace(request.Term) ||
                request.TuitionTotal <= 0)
            {
                return BadRequest("studentNo, term ve tuitionTotal zorunlu ve pozitif olmalı.");
            }

            var record = await _context.TuitionRecords
                .FirstOrDefaultAsync(t =>
                    t.StudentNo == request.StudentNo &&
                    t.Term == request.Term);

            if (record == null)
            {
                record = new TuitionRecord
                {
                    StudentNo = request.StudentNo,
                    Term = request.Term,
                    TuitionTotal = request.TuitionTotal,
                    Balance = request.TuitionTotal
                };

                _context.TuitionRecords.Add(record);
            }
            else
            {
                record.TuitionTotal = request.TuitionTotal;
                record.Balance = request.TuitionTotal;
            }

            await _context.SaveChangesAsync();

            return Ok(record);
        }

        // POST /api/v1/Admin/tuition/batch
        [HttpPost("tuition/batch")]
        public async Task<IActionResult> AddTuitionBatch(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Lütfen en az bir satır içeren bir CSV dosyası yükleyin.");
            }

            int lineNumber = 0;
            int successCount = 0;
            int failCount = 0;

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (lineNumber == 1 && line.Contains("StudentNo", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                var parts = line.Split(',', System.StringSplitOptions.TrimEntries);

                if (parts.Length < 3)
                {
                    failCount++;
                    continue;
                }

                var studentNo = parts[0];
                var term = parts[1];

                if (!decimal.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var tuitionTotal))
                {
                    failCount++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(studentNo) ||
                    string.IsNullOrWhiteSpace(term) ||
                    tuitionTotal <= 0)
                {
                    failCount++;
                    continue;
                }

                var record = await _context.TuitionRecords
                    .FirstOrDefaultAsync(t => t.StudentNo == studentNo && t.Term == term);

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

                successCount++;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Batch işlem tamamlandı.",
                successCount,
                failCount
            });
        }

        // GET /api/v1/Admin/unpaid
        [HttpGet("unpaid")]
        public async Task<IActionResult> GetUnpaidTuitions(int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.TuitionRecords
                .Where(t => t.Balance > 0)
                .OrderBy(t => t.StudentNo)
                .ThenBy(t => t.Term);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new
            {
                page,
                pageSize,
                totalCount,
                items
            };

            return Ok(result);
        }

        // DELETE /api/v1/Admin/tuition/{studentNo}/{term}
        // Admin only + Cascade delete (tuition + payments)
        [HttpDelete("tuition/{studentNo}/{term}")]
        public async Task<IActionResult> DeleteTuition(string studentNo, string term)
        {
            // 🔐 Admin only kontrolü (JWT içindeki username)
            var username = User.Identity?.Name;
            if (!string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid("Bu işlemi sadece admin kullanıcısı yapabilir.");
            }

            // Tuition kaydını bul
            var record = await _context.TuitionRecords
                .FirstOrDefaultAsync(t => t.StudentNo == studentNo && t.Term == term);

            if (record == null)
                return NotFound(new { message = "Silinecek tuition kaydı bulunamadı." });

            // İlgili payments kayıtlarını bul
            var payments = await _context.Payments
                .Where(p => p.StudentNo == studentNo && p.Term == term)
                .ToListAsync();

            if (payments.Any())
                _context.Payments.RemoveRange(payments);

            // Tuition kaydını sil
            _context.TuitionRecords.Remove(record);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Tuition kaydı ve ilgili ödeme kayıtları silindi.",
                deletedPaymentCount = payments.Count
            });
        }
    }
}
