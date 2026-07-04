using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BL.DTOs;
using BL.Services.Abstraction;
using DAL.Database;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace PL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(
            IPaymentService paymentService,
            ISubscriptionService subscriptionService,
            UserManager<User> userManager,
            AppDbContext context,
            IConfiguration configuration)
        {
            _paymentService = paymentService;
            _subscriptionService = subscriptionService;
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
        }

        /// <summary>
        /// Get all active subscription plans
        /// </summary>
        [HttpGet("subscription-plans")]
        public async Task<IActionResult> GetSubscriptionPlans()
        {
            try
            {
                var plans = await _subscriptionService.GetActiveSubscriptionPlansAsync();
                return Ok(new { success = true, data = plans });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get current user's subscription info
        /// </summary>
        [HttpGet("my-subscription")]
        [Authorize]
        public async Task<IActionResult> GetMySubscription()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var subscription = await _subscriptionService.GetUserSubscriptionAsync(userId);
                return Ok(new { success = true, data = subscription });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Create checkout session for subscription
        /// </summary>
        [HttpPost("create-subscription-session")]
        [Authorize]
        public async Task<IActionResult> CreateSubscriptionSession([FromBody] CreateSubscriptionSessionDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var response = await _paymentService.CreateSubscriptionCheckoutSessionAsync(userId, dto.PlanId);
                return Ok(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Create checkout session for minutes purchase
        /// </summary>
        [HttpPost("create-minutes-session")]
        [Authorize]
        public async Task<IActionResult> CreateMinutesSession([FromBody] CreateMinutesSessionDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var response = await _paymentService.CreateMinutePurchaseCheckoutSessionAsync(userId, dto.Minutes);
                return Ok(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Verify payment and complete subscription/minutes purchase
        /// </summary>
        [HttpPost("verify-payment")]
        [Authorize]
        public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var success = await _paymentService.HandlePaymentSuccessAsync(dto.SessionId, userId);
                
                if (success)
                {
                    var payment = await _context.Payments.FirstOrDefaultAsync(p => p.StripePaymentIntentId == dto.SessionId);

                    return Ok(new
                    {
                        success = true,
                        paymentType = payment.PaymentType.ToString()
                    });
                }

                return BadRequest(new { success = false, error = "Payment verification failed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get user's payment history
        /// </summary>
        [HttpGet("my-payments")]
        [Authorize]
        public async Task<IActionResult> GetMyPayments()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var payments = await _subscriptionService.GetUserPaymentsAsync(userId);
                return Ok(new { success = true, data = payments });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Cancel active subscription
        /// </summary>
        [HttpPost("cancel-subscription")]
        [Authorize]
        public async Task<IActionResult> CancelSubscription()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var success = await _subscriptionService.CancelSubscriptionAsync(userId);
                
                if (success)
                    return Ok(new { success = true, message = "Subscription cancelled successfully" });

                return BadRequest(new { success = false, error = "No active subscription found" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }
    }

    // DTOs for Payment Controller
    public class CreateSubscriptionSessionDto
    {
        public int PlanId { get; set; }
    }

    public class CreateMinutesSessionDto
    {
        public int Minutes { get; set; }
    }

    public class VerifyPaymentDto
    {
        public string SessionId { get; set; }
    }
}
