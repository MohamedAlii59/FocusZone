using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DAL.Database;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Linq;
using System.Collections.Generic;
using BL.DTOs.ExamSession;

namespace PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamSessionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ExamSessionController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/examsession/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ExamSessionDto>>> GetByUser(string userId)
        {
            var items = await _context.ExamSessions
                .Include(es => es.StudySession)
                    .ThenInclude(ss => ss.Resource)
                .Include(es => es.SessionAnswers)
                    .ThenInclude(sa => sa.AnswerChoices)
                .Where(es => es.StudySession != null && es.StudySession.UserId == userId)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<ExamSessionDto>>(items));
        }

        // GET: api/examsession/session/{sessionId}/user/{userId}
        [HttpGet("session/{sessionId}/user/{userId}")]
        public async Task<ActionResult<ExamSessionDto>> GetBySessionAndUser(long sessionId, string userId)
        {
            var item = await _context.ExamSessions
                .Include(es => es.StudySession)
                    .ThenInclude(ss => ss.Resource)
                .Include(es => es.SessionAnswers)
                    .ThenInclude(sa => sa.AnswerChoices)
                .Where(es => es.SessionId == sessionId && es.StudySession != null && es.StudySession.UserId == userId)
                .FirstOrDefaultAsync();

            if (item == null)
                return NotFound();

            return Ok(_mapper.Map<ExamSessionDto>(item));
        }

        // GET: api/examsession/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ExamSessionDto>> Get(int id)
        {
            var item = await _context.ExamSessions
                .Include(es => es.StudySession)
                    .ThenInclude(ss => ss.Resource)
                .Include(es => es.SessionAnswers)
                    .ThenInclude(sa => sa.AnswerChoices)
                .FirstOrDefaultAsync(es => es.ExamId == id);

            if (item == null)
                return NotFound();

            return Ok(_mapper.Map<ExamSessionDto>(item));
        }

        // POST: api/examsession
        // Accepts full ExamSession with SessionAnswers and AnswerChoices
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ExamSessionDto>> Post([FromBody] ExamSessionDto dto)
        {
            if (dto == null)
                return BadRequest();

            var entity = _mapper.Map<ExamSession>(dto);

            // Ensure PKs are zero so EF treats them as new
            entity.ExamId = 0;
            if (entity.SessionAnswers != null)
            {
                foreach (var sa in entity.SessionAnswers)
                {
                    sa.AnswerId = 0;
                    if (sa.AnswerChoices != null)
                    {
                        foreach (var ac in sa.AnswerChoices)
                        {
                            ac.ChoiceId = 0;
                        }
                    }
                }
            }

            _context.ExamSessions.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = entity.ExamId }, _mapper.Map<ExamSessionDto>(entity));
        }

        // PUT: api/examsession/{id}
        // Replaces ExamSession and its nested SessionAnswers/AnswerChoices
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody] ExamSessionDto dto)
        {
            if (dto == null || id != dto.ExamId)
                return BadRequest();

            var existing = await _context.ExamSessions
                .Include(es => es.StudySession)
                    .ThenInclude(ss => ss.Resource)
                .Include(es => es.SessionAnswers)
                    .ThenInclude(sa => sa.AnswerChoices)
                .FirstOrDefaultAsync(es => es.ExamId == id);

            if (existing == null)
                return NotFound();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Update scalar fields
                existing.Score = dto.Score;
                existing.TotalQuestions = dto.TotalQuestions;
                existing.Passed = dto.Passed;
                existing.SessionId = dto.SessionId; // keep FK updated to StudySession

                // Remove old answers (and their choices) if any
                if (existing.SessionAnswers != null && existing.SessionAnswers.Any())
                {
                    _context.SessionAnswers.RemoveRange(existing.SessionAnswers);
                    await _context.SaveChangesAsync();
                }

                // Add new answers
                existing.SessionAnswers = new List<SessionAnswer>();
                if (dto.SessionAnswers != null)
                {
                    foreach (var sa in dto.SessionAnswers)
                    {
                        var newSa = new SessionAnswer
                        {
                            ExamId = existing.ExamId,
                            Topic = sa.Topic,
                            Question = sa.Question,
                            UserAnswer = sa.UserAnswer,
                            CorrectAnswer = sa.CorrectAnswer,
                            IsCorrect = sa.IsCorrect,
                            Explanation = sa.Explanation,
                            AnswerChoices = new List<AnswerChoice>()
                        };

                        if (sa.AnswerChoices != null)
                        {
                            foreach (var ac in sa.AnswerChoices)
                            {
                                newSa.AnswerChoices.Add(new AnswerChoice
                                {
                                    ChoiceKey = ac.ChoiceKey,
                                    ChoiceValue = ac.ChoiceValue
                                });
                            }
                        }

                        existing.SessionAnswers.Add(newSa);
                    }
                }

                _context.ExamSessions.Update(existing);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // DELETE: api/examsession/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.ExamSessions.FindAsync(id);
            if (existing == null)
                return NotFound();

            _context.ExamSessions.Remove(existing);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}