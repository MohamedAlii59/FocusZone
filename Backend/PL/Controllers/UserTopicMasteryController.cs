using AutoMapper;
using BL.DTOs.UserTopicMastery;
using BL.Pagination;
using BL.Services.Abstraction;
using DAL.Database;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserTopicMasteryController : ControllerBase
    {
        private readonly IUserTopicMasteryService _service;

        public UserTopicMasteryController(IUserTopicMasteryService service)
        {
            _service = service;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> Get(string userId,[FromQuery] PaginationParams pagination)
        {
            var result = await _service.GetByUserAsync(userId, pagination);

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(UserTopicMasteryDto dto)
        {
            await _service.CreateAsync(dto);

            return Ok();
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update(UserTopicMasteryDto dto)
        {
            await _service.UpdateAsync(dto);

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{userId}/{topicId}")]
        public async Task<IActionResult> Delete( string userId,  int topicId)
        {
            await _service.DeleteAsync(userId, topicId);

            return NoContent();
        }
    }
}