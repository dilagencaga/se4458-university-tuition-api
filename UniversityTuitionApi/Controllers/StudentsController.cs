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
    [Route("api/v1/students")]
    public class StudentsController : ControllerBase
    {
        private readonly UniversityContext _context;

        public StudentsController(UniversityContext context)
        {
            _context = context;
        }

        // POST /api/v1/students
        // Body:
        // {
        //   "studentNo": "99999",
        //   "fullName": "Safinaz"
        // }
        [HttpPost]
        public async Task<ActionResult<Student>> CreateStudent([FromBody] Student student)
        {
            if (string.IsNullOrWhiteSpace(student.StudentNo) ||
                string.IsNullOrWhiteSpace(student.FullName))
            {
                return BadRequest("studentNo ve fullName zorunlu.");
            }

            bool exists = await _context.Students
                .AnyAsync(s => s.StudentNo == student.StudentNo);

            if (exists)
            {
                return Conflict("Bu studentNo zaten kayıtlı.");
            }

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return Ok(student);
        }

        // GET /api/v1/students?page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetStudents(int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Students
                .OrderBy(s => s.StudentNo);

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

        // GET /api/v1/students/{studentNo}
        [HttpGet("{studentNo}")]
        public async Task<ActionResult<Student>> GetStudentByNo(string studentNo)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNo == studentNo);

            if (student == null)
                return NotFound();

            return Ok(student);
        }

        // DELETE /api/v1/students/{studentNo}
        // Açıklama:
        //  - Sadece admin kullanıcı silebilir.
        //  - Öğrenciyi silerken o öğrenciye ait tüm TuitionRecords ve Payments kayıtları da silinir (cascade).
        [HttpDelete("{studentNo}")]
        [Authorize] // giriş zorunlu
        public async Task<IActionResult> DeleteStudent(string studentNo)
        {
            // 👉 Admin only kontrolü (JWT içindeki username)
            var username = User.Identity?.Name;
            if (!string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid("Bu işlemi sadece admin kullanıcısı yapabilir.");
            }

            // Öğrenciyi bul
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNo == studentNo);

            if (student == null)
            {
                return NotFound(new { message = "Öğrenci bulunamadı." });
            }

            // Öğrenciye ait tüm tuition kayıtları
            var tuitions = await _context.TuitionRecords
                .Where(t => t.StudentNo == studentNo)
                .ToListAsync();

            // Öğrenciye ait tüm payment kayıtları
            var payments = await _context.Payments
                .Where(p => p.StudentNo == studentNo)
                .ToListAsync();

            if (payments.Any())
            {
                _context.Payments.RemoveRange(payments);
            }

            if (tuitions.Any())
            {
                _context.TuitionRecords.RemoveRange(tuitions);
            }

            _context.Students.Remove(student);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Öğrenci, ilgili tüm tuition ve payment kayıtlarıyla birlikte silindi.",
                deletedTuitionCount = tuitions.Count,
                deletedPaymentCount = payments.Count
            });
        }
    }
}
