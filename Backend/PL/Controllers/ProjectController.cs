using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DAL.Database;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using BL.DTOs.Project;
using AutoMapper;
using System.Linq;
using System.Collections.Generic;

namespace PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProjectController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetByUser(string userId)
        {
            var items = await _context.Projects.Where(p => p.UserId == userId).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<ProjectDto>>(items));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ProjectDto>> Post([FromBody] ProjectDto dto)
        {
            var entity = _mapper.Map<Project>(dto);
            _context.Projects.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetByUser), new { userId = entity.UserId }, _mapper.Map<ProjectDto>(entity));
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(long id, [FromBody] ProjectDto dto)
        {
            if (id != dto.ProjectId)
                return BadRequest();

            var entity = await _context.Projects.FindAsync(id);
            if (entity == null)
                return NotFound();

            _mapper.Map(dto, entity);
            _context.Projects.Update(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _context.Projects.FindAsync(id);
            if (entity == null)
                return NotFound();

            _context.Projects.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}