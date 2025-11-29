using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityTuitionApi.Data;
using UniversityTuitionApi.Models;

namespace UniversityTuitionApi.Controllers
{
    [ApiController]
    [Route("api/v1/tuition")]
    public class TuitionController : ControllerBase
    {
        private readonly UniversityContext _context;

        public TuitionController(UniversityContext context)
        {
            _context = context;
        }

        // GET /api/v1/tuition/{studentNo}
        // Tek dönemlik tuition kaydını döner (en son kayıt)
        [HttpGet("{studentNo}")]
        public async Task<ActionResult<TuitionRecord>> GetTuitionForStudent(string studentNo)
        {
            var tuition = await _context.TuitionRecords
                .Where(t => t.StudentNo == studentNo)
                .OrderByDescending(t => t.Term)
                .FirstOrDefaultAsync();

            if (tuition == null)
                return NotFound();

            return Ok(tuition);
        }
    }
}
