using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;

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
                return BadRequest("Url is required");

            try
            {
                var uri = new Uri(url);
                var host = uri.Host.ToLower();

                // YouTube
                if (host.Contains("youtube.com") || host.Contains("youtu.be"))
                {
                    var oembedUrl =
                        $"https://www.youtube.com/oembed?url={Uri.EscapeDataString(url)}&format=json";

                    var result = await _httpClient.GetAsync(oembedUrl);

                    return Ok(new
                    {
                        type = "youtube",
                        canEmbed = result.IsSuccessStatusCode
                    });
                }

                using var request = new HttpRequestMessage(HttpMethod.Head, url);
                using var response = await _httpClient.SendAsync(request);

                var contentType = response.Content.Headers.ContentType?.MediaType ?? "";

                // PDF
                if (contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase) ||
                    uri.AbsolutePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return Ok(new
                    {
                        type = "pdf",
                        canEmbed = true
                    });
                }

                // Website / Blog
                var xFrameOptions = response.Headers
                    .FirstOrDefault(h => h.Key.Equals(
                        "X-Frame-Options",
                        StringComparison.OrdinalIgnoreCase))
                    .Value
                    ?.FirstOrDefault();

                var csp = response.Headers
                    .FirstOrDefault(h => h.Key.Equals(
                        "Content-Security-Policy",
                        StringComparison.OrdinalIgnoreCase))
                    .Value
                    ?.FirstOrDefault();

                var canEmbed =
                    string.IsNullOrEmpty(xFrameOptions) &&
                    !(csp?.Contains("frame-ancestors",
                        StringComparison.OrdinalIgnoreCase) ?? false);

                return Ok(new
                {
                    type = "article",
                    canEmbed
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
