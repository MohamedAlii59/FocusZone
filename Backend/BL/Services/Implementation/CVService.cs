using BL.DTOs.CV;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
namespace BL.Services.Implementation
{
    public class CVService
    {
        readonly HttpClient _httpClient;
        readonly IConfiguration _configuration;
        public CVService(HttpClient client , IConfiguration config)
        {
            _httpClient = client;
            _configuration = config;
        }
        // get the latext from Ai - agents 
        public async Task<LatexResponseDto> GetLatex(GetLatexRequestDto dto, string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = JsonContent.Create(dto);
            var res = await _httpClient.SendAsync(request);
            if (!res.IsSuccessStatusCode)
                throw new Exception("Talking To Agents Dead !");
            return await res.Content.ReadFromJsonAsync<LatexResponseDto>();
        }
        
        // convert The latex To Pdf 
        public async Task<byte[]> ConvertLatexToPDF(LatexPostPDFConveterRequestDto dto)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _configuration["LatexConverter:BaseUrl"]?.ToString());
            request.Headers.Add("x-api-key", _configuration["LatexConverter:APIKey"]?.ToString());
            request.Content = JsonContent.Create(dto);
            var res = await _httpClient.SendAsync(request);
            if (!res.IsSuccessStatusCode)
                throw new Exception("Conveter Dead !");
            return await res.Content.ReadAsByteArrayAsync();
        } 
    }
}
