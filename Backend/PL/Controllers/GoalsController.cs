using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DAL.Database;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using BL.DTOs;
using AutoMapper;
using System.Linq;
using System.Collections.Generic;

namespace PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoalsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public GoalsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Goal>>> GetByUser(string userId)
        {
            var items = await _context.Goals.Where(g => g.UserId == userId).ToListAsync();
            return Ok(items);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Goal>> Post([FromBody] GoalDTO model)
        {
            if (model == null)
                return BadRequest();

            var goal = _mapper.Map<Goal>(model);
            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetByUser), new { userId = model.UserId }, model);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(long id, [FromBody] GoalDTO model)
        {
            if (id != model.GoalId)
                return BadRequest();

            var entity = await _context.Goals.FindAsync(id);
            if (entity == null)
                return NotFound();

            entity.Title = model.Title;
            entity.Description = model.Description;
            entity.Priority = model.Priority;
            entity.Category = model.Category;

            _context.Goals.Update(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _context.Goals.FindAsync(id);
            if (entity == null)
                return NotFound();

            _context.Goals.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}