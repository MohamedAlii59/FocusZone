using AutoMapper;
using DAL.Database;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public DashboardController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("dashboard")]
        [Authorize]
        public async Task<ActionResult<UserDashboardDto>> GetDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var studySessionsCount = await _context.StudySessions.CountAsync(s => s.UserId == userId);

            var passedExamsCount = await _context.ExamSessions.CountAsync(e =>e.Passed == true && e.StudySession != null && e.StudySession.UserId == userId);

            var userTopicMasteryCount = await _context.UserTopicMasteries.CountAsync(u => u.UserId == userId);

            var passRate = studySessionsCount == 0? 0 : Math.Round((double)passedExamsCount / studySessionsCount * 100, 1);

            var result = new UserDashboardDto
            {
                StudySessionsCount = studySessionsCount,
                PassedExamsCount = passedExamsCount,
                UserTopicMasteryCount = userTopicMasteryCount,
                PassRate = passRate
            };

            return Ok(result);
        }
    }
}
