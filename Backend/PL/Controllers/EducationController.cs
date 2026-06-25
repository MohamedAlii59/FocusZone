using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DAL.Database;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using BL.DTOs.Education;
using AutoMapper;
using System.Linq;
using System.Collections.Generic;

namespace PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EducationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public EducationController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<EducationDto>>> GetByUser(string userId)
        {
            var items = await _context.Educations.Where(e => e.UserId == userId).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<EducationDto>>(items));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<EducationDto>> Post([FromBody] EducationDto dto)
        {
            var entity = _mapper.Map<Education>(dto);
            _context.Educations.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetByUser), new { userId = entity.UserId }, _mapper.Map<EducationDto>(entity));
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(long id, [FromBody] EducationDto dto)
        {
            if (id != dto.EducationId)
                return BadRequest();

            var entity = await _context.Educations.FindAsync(id);
            if (entity == null)
                return NotFound();

            _mapper.Map(dto, entity);
            _context.Educations.Update(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _context.Educations.FindAsync(id);
            if (entity == null)
                return NotFound();

            _context.Educations.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}