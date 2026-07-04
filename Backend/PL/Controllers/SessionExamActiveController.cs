using System.Linq;
using System.Threading.Tasks;
using DAL.Database;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionExamActiveController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SessionExamActiveController(AppDbContext context)
        {
            _context = context;
        }

        // Get session exam active entry / status for a session
        [HttpGet("session/{sessionId}")]
        [Authorize]
        public async Task<IActionResult> GetBySessionId(long sessionId)
        {
            var entry = await _context.SessionExamActives
                .Where(s => s.SessionId == sessionId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (entry == null)
            {
                return Ok(new { sessionId, isActive = false });
            }

            return Ok(new { sessionId, isActive = entry.IsActive });
        }

        // Create a new session exam active entry. If isActive=true, deactivate other entries for the same session.
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSessionExamActiveDto dto)
        {
            if (dto == null) return BadRequest();
            // Enforce 1:1 relation - if an entry already exists for this session, return conflict
            var existing = await _context.SessionExamActives
                .Where(s => s.SessionId == dto.SessionId)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                return Conflict(new { message = "A SessionExamActive entry for this session already exists." });
            }

            var entity = new SessionExamActive
            {
                SessionId = dto.SessionId,
                IsActive = dto.IsActive
            };

            _context.SessionExamActives.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBySessionId), new { sessionId = entity.SessionId }, entity);
        }

        // (GetBySessionId implemented above)

        // update session exam active status. set its status with the new value.
        [Authorize]
        [HttpPut("session/{sessionId}")]
        public async Task<IActionResult> Update(long sessionId, [FromBody] UpdateSessionExamActiveDto dto)
        {
            var entry = await _context.SessionExamActives
                .Where(s => s.SessionId == sessionId)
                .FirstOrDefaultAsync();

            if (entry == null) return NotFound();

            entry.IsActive = dto.IsActive;
            _context.SessionExamActives.Update(entry);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public record CreateSessionExamActiveDto(long SessionId, bool IsActive);
    public record UpdateSessionExamActiveDto(bool IsActive);
}
