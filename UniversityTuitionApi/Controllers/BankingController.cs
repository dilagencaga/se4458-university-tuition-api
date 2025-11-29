using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityTuitionApi.Data;

namespace UniversityTuitionApi.Controllers
{
    [ApiController]
    [Route("api/v1/banking")]
    [Authorize]  // 🔐 Banking tarafı korumalı
    public class BankingController : ControllerBase
    {
        private readonly UniversityContext _context;

        public BankingController(UniversityContext context)
        {
            _context = context;
        }

        // GET /api/v1/banking/tuition/{studentNo}
        [HttpGet("tuition/{studentNo}")]
        public async Task<IActionResult> GetBankingTuition(string studentNo)
        {
            var tuition = await _context.TuitionRecords
                .Where(t => t.StudentNo == studentNo)
                .OrderByDescending(t => t.Term)
                .FirstOrDefaultAsync();

            if (tuition == null)
                return NotFound();

            var result = new
            {
                studentNo = tuition.StudentNo,
                term = tuition.Term,
                tuitionTotal = tuition.TuitionTotal,
                balance = tuition.Balance
            };

            return Ok(result);
        }
    }
}
