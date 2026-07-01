
using BL.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace PL.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private readonly IResourceCheckService _resourceCheckService;

        public ResourcesController(IResourceCheckService resourceCheckService)
        {
            _resourceCheckService = resourceCheckService;
        }

        [HttpGet("resource/check")]
        public async Task<IActionResult> CheckResource([FromQuery] string url, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("Url is required.");

            var result = await _resourceCheckService.CheckAsync(url, ct);
            return Ok(result);
        }
    }
}