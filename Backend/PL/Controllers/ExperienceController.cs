using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DAL.Database;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using BL.DTOs.Experience;
using AutoMapper;
using System.Linq;
using System.Collections.Generic;

namespace PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExperienceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ExperienceController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ExperienceDto>>> GetByUser(string userId)
        {
            var items = await _context.Experiences.Where(e => e.UserId == userId).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<ExperienceDto>>(items));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ExperienceDto>> Post([FromBody] ExperienceDto dto)
        {
            var entity = _mapper.Map<Experience>(dto);
            _context.Experiences.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetByUser), new { userId = entity.UserId }, _mapper.Map<ExperienceDto>(entity));
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(long id, [FromBody] ExperienceDto dto)
        {
            if (id != dto.ExperienceId)
                return BadRequest();

            var entity = await _context.Experiences.FindAsync(id);
            if (entity == null)
                return NotFound();

            _mapper.Map(dto, entity);
            _context.Experiences.Update(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _context.Experiences.FindAsync(id);
            if (entity == null)
                return NotFound();

            _context.Experiences.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}