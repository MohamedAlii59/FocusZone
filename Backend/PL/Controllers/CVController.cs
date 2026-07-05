using BL.DTOs.CV;
using BL.Services.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CVController : ControllerBase
    {
        private readonly CVService _cvService;
        private readonly IConfiguration _configuration;
        public CVController(CVService cvService, IConfiguration configuration)
        {
            _cvService = cvService;
            _configuration = configuration;
        }

        [HttpPost("generate")]
        [Authorize(Roles = "PaidUser")]
        public async Task<IActionResult> GenerateCV([FromBody] GetLatexRequestDto request)
        {
            if (request.user_id != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return BadRequest("requested user id Malformed");
            try
            {
                // Generate the LaTeX using the AI service
                var latexResponse = await _cvService.GetLatex(
                    request,
                    _configuration["Agents:GenerateLatexUrl"]!.ToString()
                );

                // Convert the LaTeX to PDF
                var pdfBytes = await _cvService.ConvertLatexToPDF(
                    new LatexPostPDFConveterRequestDto
                    {
                        content = latexResponse.latex
                    });

                // Return the PDF
                return File(
                    pdfBytes,
                    "application/pdf",
                    $"CV-{request.user_id}.pdf");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    message = "Unable to communicate with the external service.",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Failed to generate the CV.",
                    error = ex.Message
                });
            }
        }
    }
}
