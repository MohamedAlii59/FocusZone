
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DAL.Entities;
using DAL.Database;
using BL.DTOs;
using BL.Services.Abstraction;

namespace PL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserService _userService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IUserService userService, IJwtTokenService jwtTokenService, IEmailService emailService, IConfiguration configuration, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// Register a new user with email and password
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.RegisterUserAsync(model, User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System");
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                // Token expires after 1 day for newly registered users
                var token = await _jwtTokenService.GenerateTokenAsync(user, 24 * 60);
                return Ok(new { message = "User registered successfully", userId = user.Id, token = token });
            }

            return BadRequest(new { errors = result.Errors });
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.IsDeleted)
                return Unauthorized(new { message = "Invalid email or password" });

            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);
            if (result.Succeeded)
            {
                // Token expiry: 1 day by default, 7 days when RememberMe is true
                int? tokenExpirationMinutes = model.RememberMe ? (int?)(7 * 24 * 60) : (int?)(24 * 60);
                var token = await _jwtTokenService.GenerateTokenAsync(user, tokenExpirationMinutes);
                return Ok(new { message = "Login successful", userId = user.Id, email = user.Email, token = token, rememberMe = model.RememberMe });
            }

            return Unauthorized(new { message = "Invalid email or password" });
        }

        /// <summary>
        /// Initiate external login(Google, GitHub)
        /// </summary>
        //[HttpPost("external-login/{provider}")]
        //public IActionResult ExternalLogin(string provider)
        //{
        //    var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth");
        //    var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        //    return Challenge(properties, provider);
        //}



        [HttpGet("external-login/{provider}")]
        public IActionResult ExternalLogin(string provider)
        {
            if (string.IsNullOrEmpty(provider))
                return BadRequest(new { error = "Provider is required" });

      
            var schemes = _signInManager.GetExternalAuthenticationSchemesAsync().Result;

            
            var targetScheme = schemes.FirstOrDefault(s =>
                s.Name.Equals(provider, StringComparison.OrdinalIgnoreCase));

           
            if (targetScheme == null)
            {
                return BadRequest(new { error = $"Provider '{provider}' is not supported." });
            }

           
            var schemeName = targetScheme.Name;

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", null, Request.Scheme);
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(schemeName, redirectUrl);

            return new ChallengeResult(schemeName, properties);
        }





        /// <summary>
        /// Callback for external login providers
        /// </summary>
        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
                return BadRequest(new { error = remoteError });

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return BadRequest(new { error = "Error loading external login information" });

            // Check if user already exists with this external login
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            if (user == null)
            {
                // Extract user information from external provider
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);

                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                {
                    var fullName = info.Principal.FindFirstValue("urn:github:name")
                                   ?? info.Principal.FindFirstValue(ClaimTypes.Name)
                                   ?? email?.Split('@')[0]
                                   ?? "User";

                    var parts = fullName.Trim().Split(' ', 2);
                    firstName = parts[0];
                    lastName = parts.Length > 1 ? parts[1] : parts[0];
                }

                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    // Add external login to existing user
                    var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                    if (!addLoginResult.Succeeded)
                        return BadRequest(new { errors = addLoginResult.Errors });
                    user = existingUser;
                }
                else
                {
                    // Create new user from external login with fetched data only
                    user = await _userService.CreateExternalUserAsync(email, firstName, lastName);

                    // Add external login to user
                    var result = await _userManager.AddLoginAsync(user, info);
                    if (!result.Succeeded)
                        return BadRequest(new { errors = result.Errors });
                }
            }

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Generate and return JWT token
            // External login tokens expire after 1 day by default
            var token = await _jwtTokenService.GenerateTokenAsync(user, 24 * 60);

          
            var frontendUrl = $"{_configuration["AppSettings:FrontendUrl"]}/external-login-callback?token={token}&email={Uri.EscapeDataString(user.Email)}";

            return Redirect(frontendUrl);
        }

        /// <summary>
        /// Logout
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logout successful" });
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var userResponse = await _userService.GetUserByIdAsync(userId);
            return Ok(userResponse);
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UserResponseDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _userService.UpdateUserAsync(userId, model);
            if (result.Succeeded)
                return Ok(new { message = "Profile updated successfully" });

            return BadRequest(new { errors = result.Errors });
        }

        /// <summary>
        /// Request password reset - sends reset token via email
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Query directly from database by Email column
                // FindByEmailAsync uses NormalizedEmail which may not be set properly
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email && !u.IsDeleted);

                // If user not found or is deleted, still return success message (security best practice)
                if (user == null)
                {
                    return Ok(new { message = "If an account exists with this email, a password reset link has been sent" });
                }

                // Generate reset token using UserManager
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Build reset URL - frontend will handle the actual reset page
              
                var encodedToken = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(resetToken));

             
                var resetUrl = $"{_configuration["AppSettings:FrontendUrl"]}/auth/reset-password?email={Uri.EscapeDataString(user.Email)}&token={encodedToken}";

                // Send reset email
                var htmlBody = $@"
                    <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                                <h2 style='color: #333;'>Password Reset Request</h2>
                                <p>You have requested to reset your password for your StuckIn account. Click the button below to proceed:</p>
                                <p style='text-align: center; margin: 30px 0;'>
                                    <a href='{resetUrl}' target='_self' style='background-color: #4CAF50; color: white; padding: 12px 30px; text-decoration: none; border-radius: 4px; display: inline-block; font-weight: bold;'>
                                        Reset Password
                                    </a>
                                </p>
                                <p style='color: #666; font-size: 14px;'>
                                    Or copy and paste this link in your browser:<br>
                                    <a href='{resetUrl}' target='_self' style='color: #4CAF50; word-break: break-all;'>{resetUrl}</a>
                                </p>
                                <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
                                <p style='color: #999; font-size: 12px;'>
                                    <strong>Security Notice:</strong> This link will expire in 24 hours.<br>
                                    If you did not request this password reset, please ignore this email and do not share this link with anyone.
                                </p>
                            </div>
                        </body>
                    </html>";

                await _emailService.SendEmailAsync(user.Email, "Password Reset Request", htmlBody);

                return Ok(new { message = "If an account exists with this email, a password reset link has been sent" });
            }
            catch (Exception ex)
            {
                // Log the exception here if you have logging configured
                return BadRequest(new { error = "Failed to process password reset request", details = ex.Message });
            }
        }

        /// <summary>
        /// Reset password using reset token
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Query user directly from database by Email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email && !u.IsDeleted);

                if (user == null)
                    return BadRequest(new { error = "User not found" });

              
                var decodedTokenBytes = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(model.Token);
                var originalToken = System.Text.Encoding.UTF8.GetString(decodedTokenBytes);

             
                var result = await _userManager.ResetPasswordAsync(user, originalToken, model.NewPassword);

                if (result.Succeeded)
                    return Ok(new { message = "Password has been reset successfully. You can now login with your new password" });

                return BadRequest(new { errors = result.Errors });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to reset password", details = ex.Message });
            }
        }
    }
}
