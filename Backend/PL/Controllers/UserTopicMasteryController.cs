using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DAL.Database;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using BL.DTOs.UserTopicMastery;
using AutoMapper;
using System.Linq;
using System.Collections.Generic;

namespace PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserTopicMasteryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserTopicMasteryController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserTopicMasteryDto>>> GetByUser(string userId)
        {
            var items = await _context.UserTopicMasteries.Where(utm => utm.UserId == userId).Include(x=>x.Topic).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<UserTopicMasteryDto>>(items));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<UserTopicMasteryDto>> Post([FromBody] UserTopicMasteryDto dto)
        {
            var entity = _mapper.Map<UserTopicMastery>(dto);
            _context.UserTopicMasteries.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetByUser), new { userId = entity.UserId }, _mapper.Map<UserTopicMasteryDto>(entity));
        }

        [HttpPut("{userId}/{topicId}")]
        [Authorize]
        public async Task<IActionResult> Put(string userId, int topicId, [FromBody] UserTopicMasteryDto dto)
        {
            if (userId != dto.UserId || topicId != dto.TopicId)
                return BadRequest();

            var entity = await _context.UserTopicMasteries.FindAsync(userId, topicId);
            if (entity == null)
                return NotFound();

            _mapper.Map(dto, entity);
            _context.UserTopicMasteries.Update(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{userId}/{topicId}")]
        [Authorize]
        public async Task<IActionResult> Delete(string userId, int topicId)
        {
            var entity = await _context.UserTopicMasteries.FindAsync(userId, topicId);
            if (entity == null)
                return NotFound();

            _context.UserTopicMasteries.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}