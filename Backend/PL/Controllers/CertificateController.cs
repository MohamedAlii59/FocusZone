using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DAL.Database;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using BL.DTOs.Certificate;
using AutoMapper;
using System.Linq;
using System.Collections.Generic;

namespace PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificateController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CertificateController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<CertificateDto>>> GetByUser(string userId)
        {
            var items = await _context.Certificates.Where(e => e.UserId == userId).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<CertificateDto>>(items));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CertificateDto>> Post([FromBody] CertificateDto dto)
        {
            var entity = _mapper.Map<Certificate>(dto);
            _context.Certificates.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetByUser), new { userId = entity.UserId }, _mapper.Map<CertificateDto>(entity));
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(long id, [FromBody] CertificateDto dto)
        {
            if (id != dto.CertificateId)
                return BadRequest();

            var entity = await _context.Certificates.FindAsync(id);
            if (entity == null)
                return NotFound();

            _mapper.Map(dto, entity);
            _context.Certificates.Update(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _context.Certificates.FindAsync(id);
            if (entity == null)
                return NotFound();

            _context.Certificates.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}