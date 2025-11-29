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
        // Body:
        // {
        //   "studentNo": "99999",
        //   "term": "2025-Fall",
        //   "tuitionTotal": 10000
        // }
        [HttpPost("tuition")]
        public async Task<ActionResult<TuitionRecord>> CreateOrUpdateTuition([FromBody] TuitionRecord request)
        {
            if (string.IsNullOrWhiteSpace(request.StudentNo) ||
                string.IsNullOrWhiteSpace(request.Term) ||
                request.TuitionTotal <= 0)
            {
                return BadRequest("studentNo, term ve tuitionTotal zorunlu ve pozitif olmalı.");
            }

            // Aynı studentNo + term varsa güncelle, yoksa oluştur
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
                // Basit senaryo: yeni dönem ücreti geldiyse bakiyeyi sıfırdan başlat
                record.Balance = request.TuitionTotal;
            }

            await _context.SaveChangesAsync();

            return Ok(record);
        }

        // GET /api/v1/Admin/unpaid?page=1&pageSize=10
        // Ödenmemiş bakiyesi olan öğrencileri paging ile döner
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
    }
}
