using AutoMapper;
using BL.DTOs.ExamSession;
using BL.Pagination;
using BL.Services.Abstraction;
using BL.Services.Implementation;
using DAL.Database;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExamSessionController : ControllerBase
    {
        private readonly IExamSessionService _service;

        public ExamSessionController(IExamSessionService service)
        {
            _service = service;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(string userId,[FromQuery] PaginationParams pagination, [FromQuery] string? search, [FromQuery] string? type)
        {
            var result = await _service.GetByUserAsync(userId, pagination,search,type);

            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _service.GetAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }


        [HttpGet("session/{sessionId}/user/{userId}")]
        public async Task<IActionResult> GetBySessionAndUser(  long sessionId, string userId)
        {
            var result = await _service.GetBySessionAndUserAsync(sessionId, userId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }



        [Authorize]
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveSession()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))return Unauthorized();

            var sessionId = await _service.GetActiveSessionIdAsync(userId);

            return Ok(new
            {
                hasActiveSession = sessionId != null,
                sessionId
            });
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(AddExamSessionDto dto)
        {
            var exam = await _service.CreateAsync(dto);

            return CreatedAtAction(nameof(Get),
                new { id = exam.ExamId }, exam);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ExamSessionDto dto)
        {
            await _service.UpdateAsync(id, dto);

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);

            return NoContent();
        }
    }
}