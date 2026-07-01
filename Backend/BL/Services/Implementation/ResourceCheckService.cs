using BL.DTOs;
using BL.Services.Abstraction;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace BL.Services.Implementation
{

    public sealed class ResourceCheckService : IResourceCheckService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ResourceCheckService> _logger;

        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(6);
        private const int MaxRedirects = 3;

        private static readonly Regex DriveFileIdRegex =
            new(@"(?:/d/|[?&]id=)([-\w]{20,})", RegexOptions.Compiled);

        private static readonly Regex YouTubeIdRegex =
            new(@"(?:v=|youtu\.be/|/embed/|/shorts/)([\w-]{11})", RegexOptions.Compiled);

        public ResourceCheckService(IHttpClientFactory httpClientFactory, ILogger<ResourceCheckService> logger)
        {
            // IMPORTANT: register this named client in Program.cs WITHOUT auto-redirect,
            // so we can validate the host of every hop ourselves:
            //
            // builder.Services.AddHttpClient("SafeResourceClient")
            //     .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            //     {
            //         AllowAutoRedirect = false,
            //         ConnectTimeout = TimeSpan.FromSeconds(5)
            //     });
            _httpClient = httpClientFactory.CreateClient("SafeResourceClient");
            _logger = logger;
        }

        public async Task<ResourceCheckResult> CheckAsync(string url, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                return ResourceCheckResult.Unknown();

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return ResourceCheckResult.Unknown();
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(RequestTimeout);

            try
            {
                var host = uri.Host.ToLowerInvariant();

                if (host is "drive.google.com" or "docs.google.com")
                    return await CheckGoogleDriveAsync(url, cts.Token);

                if (IsExactOrSubdomain(host, "youtube.com") || host == "youtu.be")
                    return await CheckYouTubeAsync(url, uri, cts.Token);

                return await CheckGenericAsync(uri, cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Resource check timed out for {Url}", url);
                return ResourceCheckResult.Unknown();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Resource check network error for {Url}", url);
                return ResourceCheckResult.Unknown();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error checking resource {Url}", url);
                return ResourceCheckResult.Unknown();
            }
        }

        private async Task<ResourceCheckResult> CheckYouTubeAsync(string url, Uri uri, CancellationToken ct)
        {
            if (!await IsHostSafeAsync(uri, ct))
                return ResourceCheckResult.Unknown();

            var oembed = $"https://www.youtube.com/oembed?url={Uri.EscapeDataString(url)}&format=json";
            var result = await _httpClient.GetAsync(oembed, ct);

            if (!result.IsSuccessStatusCode)
                return new ResourceCheckResult { Type = "youtube", CanEmbed = false, Url = null };

            var idMatch = YouTubeIdRegex.Match(url);
            string? embedUrl = idMatch.Success
                ? $"https://www.youtube.com/embed/{idMatch.Groups[1].Value}"
                : null;

            return new ResourceCheckResult
            {
                Type = "youtube",
                CanEmbed = embedUrl is not null,
                Url = embedUrl
            };
        }

        private async Task<ResourceCheckResult> CheckGoogleDriveAsync(string url, CancellationToken ct)
        {
            var match = DriveFileIdRegex.Match(url);
            if (!match.Success)
                return ResourceCheckResult.Unknown();

            var fileId = match.Groups[1].Value;
            var previewUrl = $"https://drive.google.com/file/d/{fileId}/preview";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, previewUrl);
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0");

                using var response = await _httpClient.SendAsync(
                    request, HttpCompletionOption.ResponseHeadersRead, ct);

                // Drive returns 200 even for "request access" pages, so status alone
                // doesn't guarantee the file is actually publicly viewable — this is
                // a best-effort check, not a guarantee.
                bool reachable = response.IsSuccessStatusCode;

                return new ResourceCheckResult
                {
                    Type = "pdf",
                    CanEmbed = reachable,
                    Url = reachable ? previewUrl : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Google Drive preview check failed for {Url}", url);
                return new ResourceCheckResult { Type = "google-drive", CanEmbed = false, Url = null };
            }
        }

        private async Task<ResourceCheckResult> CheckGenericAsync(Uri uri, CancellationToken ct)
        {
            var (finalResponse, finalUri) = await SafeGetAsync(uri, ct);

            if (finalResponse is null || finalUri is null)
                return ResourceCheckResult.Unknown();

            using (finalResponse)
            {
                if (!finalResponse.IsSuccessStatusCode)
                    return ResourceCheckResult.Unknown();

                var contentType = finalResponse.Content.Headers.ContentType?.MediaType ?? "";

                bool isPdf = contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase)
                    || finalUri.AbsolutePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

                bool isHtml = contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase);

                string? xFrame = finalResponse.Headers.TryGetValues("X-Frame-Options", out var x)
                    ? x.FirstOrDefault()
                    : null;

                string? csp = finalResponse.Headers.TryGetValues("Content-Security-Policy", out var c)
                    ? string.Join(";", c)
                    : null;

                bool attachment = finalResponse.Content.Headers.ContentDisposition?.DispositionType
                    ?.Equals("attachment", StringComparison.OrdinalIgnoreCase) == true;

                bool canEmbed =
                    string.IsNullOrWhiteSpace(xFrame) &&
                    !(csp?.Contains("frame-ancestors", StringComparison.OrdinalIgnoreCase) ?? false) &&
                    !attachment;

                var resolvedUrl = finalUri.ToString();

                if (isPdf)
                    return new ResourceCheckResult { Type = "pdf", CanEmbed = canEmbed, Url = canEmbed ? resolvedUrl : null };

                if (isHtml)
                    return new ResourceCheckResult { Type = "article", CanEmbed = canEmbed, Url = canEmbed ? resolvedUrl : null };

                return ResourceCheckResult.Unknown();
            }
        }

        /// <summary>
        /// Performs a GET-with-headers-only request, manually following redirects
        /// while re-validating that each hop resolves to a public, non-internal host.
        /// </summary>
        private async Task<(HttpResponseMessage? response, Uri? finalUri)> SafeGetAsync(Uri uri, CancellationToken ct)
        {
            var current = uri;

            for (int i = 0; i <= MaxRedirects; i++)
            {
                if (current.Scheme != Uri.UriSchemeHttp && current.Scheme != Uri.UriSchemeHttps)
                    return (null, null);

                if (!await IsHostSafeAsync(current, ct))
                    return (null, null);

                using var request = new HttpRequestMessage(HttpMethod.Get, current);
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0");

                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

                if (IsRedirect(response.StatusCode) && response.Headers.Location is not null)
                {
                    var next = response.Headers.Location.IsAbsoluteUri
                        ? response.Headers.Location
                        : new Uri(current, response.Headers.Location);

                    response.Dispose();
                    current = next;
                    continue;
                }

                return (response, current);
            }

            // Too many redirects.
            return (null, null);
        }

        private static bool IsRedirect(HttpStatusCode code) =>
            code is HttpStatusCode.Moved or HttpStatusCode.Redirect or HttpStatusCode.SeeOther
                or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect;

        /// <summary>
        /// Blocks SSRF against internal/private/loopback/link-local/metadata addresses.
        /// Resolves DNS ourselves so a "public" hostname can't rebind to an internal IP.
        /// </summary>
        private static async Task<bool> IsHostSafeAsync(Uri uri, CancellationToken ct)
        {
            try
            {
                var addresses = await Dns.GetHostAddressesAsync(uri.Host, ct);

                if (addresses.Length == 0)
                    return false;

                return addresses.All(IsPublicIp);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsPublicIp(IPAddress ip)
        {
            if (IPAddress.IsLoopback(ip)) return false;
            if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();

            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                var bytes = ip.GetAddressBytes();
                if (bytes[0] == 10) return false;                          // 10.0.0.0/8
                if (bytes[0] == 172 && bytes[1] is >= 16 and <= 31) return false; // 172.16.0.0/12
                if (bytes[0] == 192 && bytes[1] == 168) return false;      // 192.168.0.0/16
                if (bytes[0] == 169 && bytes[1] == 254) return false;      // 169.254.0.0/16 (incl. metadata)
                if (bytes[0] == 127) return false;                         // 127.0.0.0/8
                if (bytes[0] == 0) return false;                           // 0.0.0.0/8
            }
            else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal) return false;
                if (ip.Equals(IPAddress.IPv6Loopback)) return false;
                var bytes = ip.GetAddressBytes();
                if ((bytes[0] & 0xFE) == 0xFC) return false;                // fc00::/7 unique local
            }

            return true;
        }

        private static bool IsExactOrSubdomain(string host, string domain) =>
            host == domain || host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase);
    }
}
