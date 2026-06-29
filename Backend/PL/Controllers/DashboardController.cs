using AutoMapper;
using BL.Services.Abstraction;
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
        private readonly IUserDashboardService _dashboardService;

        public DashboardController(IUserDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }


        [HttpGet("dashboard")]
        [Authorize]
        public async Task<ActionResult<UserDashboardDto>> GetDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await _dashboardService.GetDashboardAsync(userId);

            return Ok(result);
        }

    }
}
