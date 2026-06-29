using DAL.Entities;


namespace BL.Services.Abstraction
{
    public interface IJwtTokenService
    {
        Task<string> GenerateTokenAsync(User user, int? expirationMinutes = null);
    }

}
