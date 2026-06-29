
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using DAL.Entities;
using BL.Services.Abstraction;

namespace BL.Services.Implementation
{
  

    public class JwtTokenService : IJwtTokenService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _defaultExpirationMinutes;
        private readonly UserManager<User> _userManager;

        public JwtTokenService(string secretKey, string issuer, string audience, int defaultExpirationMinutes, UserManager<User> userManager = null)
        {
            _secretKey = secretKey;
            _issuer = issuer;
            _audience = audience;
            _defaultExpirationMinutes = defaultExpirationMinutes;
            _userManager = userManager;
        }

        public async Task<string> GenerateTokenAsync(User user, int? expirationMinutes = null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim("FirstName", user.FirstName ?? ""),
                new Claim("LastName", user.LastName ?? ""),
                new Claim("SessionMinutes", user.SessionMinutes.ToString())
            };

            // Add roles to claims
            if (_userManager != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            // Use provided expiration minutes or default
            var minutesToExpire = expirationMinutes ?? _defaultExpirationMinutes;

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutesToExpire),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
    }
}
