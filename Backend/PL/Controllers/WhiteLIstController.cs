using BL.DTOs.WhiteList;
using DAL.Database;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WhiteListController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WhiteListController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("whiteList/{sessionId}")]
        [Authorize]
        public IActionResult GetSessionWhiteList(long sessionId)
        {
            var whiteList = _context.SessionWhiteLists
                .Where(sw => sw.SessionId == sessionId)
                .Select(sw => sw.Url)
                .ToList();

            // Check if any URLs exist for this session first
            if (whiteList == null || !whiteList.Any())
            {
                return NotFound(new { message = $"No whitelist found for Session ID {sessionId}." });
            }

            GetSessionWhiteListDto dto = new GetSessionWhiteListDto
            {
                SessionId = sessionId,
                WhiteList = whiteList
            };

            return Ok(dto);
        }

        [HttpPost("whiteList/add")]
        [Authorize]
        public IActionResult AddSessionWhiteList([FromBody] GetSessionWhiteListDto whitelist)
        {
            // Fail fast if payload is invalid
            if (whitelist?.WhiteList == null || !whitelist.WhiteList.Any())
            {
                return BadRequest("Whitelist data cannot be empty.");
            }

            // 1. Map DTO strings to entity models in memory
            var entities = whitelist.WhiteList.Select(url => new SessionWhiteList
            {
                SessionId = whitelist.SessionId,
                Url = url
            });

            // 2. Stage the entities for batch insertion
            _context.SessionWhiteLists.AddRange(entities);

            // 3. Persist to database in a single round-trip transaction
            _context.SaveChanges();

            return Ok(new { message = "Whitelist updated successfully." });
        }
    }
}