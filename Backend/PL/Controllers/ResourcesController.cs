
using Microsoft.AspNetCore.Mvc;

namespace PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ResourcesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet("resource/check")]
        public async Task<IActionResult> CheckResource([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("Url is required.");

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return Ok(new
                {
                    type = "unknown",
                    canEmbed = false
                });
            }

            try
            {
                var host = uri.Host.ToLowerInvariant();

                // YouTube

                if (host.Contains("youtube.com") || host.Contains("youtu.be"))
                {
                    var oembed =
                        $"https://www.youtube.com/oembed?url={Uri.EscapeDataString(url)}&format=json";

                    var result = await _httpClient.GetAsync(oembed);

                    return Ok(new
                    {
                        type = "youtube",
                        canEmbed = result.IsSuccessStatusCode
                    });
                }

                // Request only headers

                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                request.Headers.UserAgent.ParseAdd(
                    "Mozilla/5.0");

                using var response = await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    return Ok(new
                    {
                        type = "unknown",
                        canEmbed = false
                    });
                }

                var contentType =
                    response.Content.Headers.ContentType?.MediaType ?? "";

                // Detect PDF

                bool isPdf =
                    contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase)
                    || uri.AbsolutePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

                // Detect HTML

                bool isHtml =
                    contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase);

                // X-Frame-Options

                string? xFrame = null;

                if (response.Headers.TryGetValues("X-Frame-Options", out var x))
                    xFrame = x.FirstOrDefault();

                // CSP

                string? csp = null;

                if (response.Headers.TryGetValues("Content-Security-Policy", out var c))
                    csp = string.Join(";", c);

                // Content-Disposition

                bool attachment =
                    response.Content.Headers.ContentDisposition?.DispositionType?.Equals(
                        "attachment",
                        StringComparison.OrdinalIgnoreCase) == true;

                // Can Embed

                bool canEmbed =
                    string.IsNullOrWhiteSpace(xFrame) &&
                    !(csp?.Contains("frame-ancestors",
                        StringComparison.OrdinalIgnoreCase) ?? false) &&
                    !attachment;

                // Return PDF

                if (isPdf)
                {
                    return Ok(new
                    {
                        type = "pdf",
                        canEmbed
                    });
                }

                // Return Article

                if (isHtml)
                {
                    return Ok(new
                    {
                        type = "article",
                        canEmbed
                    });
                }

                // Unknown

                return Ok(new
                {
                    type = "unknown",
                    canEmbed = false
                });
            }
            catch
            {
                return Ok(new
                {
                    type = "unknown",
                    canEmbed = false
                });
            }
        }

    }
}
