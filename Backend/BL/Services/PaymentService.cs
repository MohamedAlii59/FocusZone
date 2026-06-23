using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using DAL.Entities;
using DAL.Database;
using BL.DTOs;

namespace BL.Services
{
    public interface IPaymentService
    {
        Task<CreatePaymentIntentResponseDto> CreateSubscriptionCheckoutSessionAsync(string userId, int planId);
        Task<CreatePaymentIntentResponseDto> CreateMinutePurchaseCheckoutSessionAsync(string userId, int minutes);
        Task<bool> HandlePaymentSuccessAsync(string sessionId, string authenticatedUserId);
        Task<SubscriptionPlan> GetSubscriptionPlanAsync(int planId);
        Task<bool> ProcessSubscriptionAsync(string userId, int planId, string sessionId);
        Task<bool> ProcessMinutesPurchaseAsync(string userId, int minutes, decimal amount);
    }

    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly string _stripeSecretKey;
        private readonly string _stripePublishableKey;
        private readonly string _appUrl;

        public PaymentService(AppDbContext context, UserManager<User> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _stripeSecretKey = configuration["Stripe:SecretKey"];
            _stripePublishableKey = configuration["Stripe:PublishableKey"];

            _appUrl = configuration["AppSettings:AppUrl"];
            StripeConfiguration.ApiKey = _stripeSecretKey;
        }

        public async Task<CreatePaymentIntentResponseDto> CreateSubscriptionCheckoutSessionAsync(string userId, int planId)
        {
            var plan = await GetSubscriptionPlanAsync(planId);
            if (plan == null)
                throw new Exception("Subscription plan not found");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception("User not found");

            try
            {
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new System.Collections.Generic.List<string> { "card" },
                    LineItems = new System.Collections.Generic.List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(plan.Price * 100),
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = plan.Name,
                                    Description = plan.Description,
                                }
                            },
                            Quantity = 1,
                        },
                    },
                    Mode = "payment",
                    SuccessUrl = $"{_appUrl}/payment-success?sessionId={{CHECKOUT_SESSION_ID}}&planId={planId}",
                    CancelUrl = $"{_appUrl}/payment-cancel",
                    CustomerEmail = user.Email,
                    Metadata = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "userId", userId },
                        { "planId", planId.ToString() },
                        { "planType", "subscription" }
                    }
                };

                var session = await new SessionService().CreateAsync(options);

                var payment = new Payment
                {
                    UserId = userId,
                    SubscriptionPlanId = planId,
                    Amount = plan.Price,
                    PaymentType = PaymentType.Subscription,
                    Status = PaymentStatus.Pending,
                    StripePaymentIntentId = session.Id,
                    CreatedOn = DateTime.UtcNow
                };

                await _context.Payments.AddAsync(payment);
                await _context.SaveChangesAsync();

                return new CreatePaymentIntentResponseDto
                {
                    CheckoutUrl = session.Url,
                    PublishableKey = _stripePublishableKey,
                    Amount = plan.Price,
                    Currency = "usd"
                };
            }
            catch (StripeException ex)
            {
                throw new Exception($"Stripe error: {ex.Message}");
            }
        }

        public async Task<CreatePaymentIntentResponseDto> CreateMinutePurchaseCheckoutSessionAsync(string userId, int minutes)
        {
            if (minutes != 300 && minutes != 1000)
                throw new Exception("Invalid minutes amount. Allowed: 300 or 1000");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception("User not found");

            decimal amount = minutes == 300 ? 2m : 5m;

            try
            {
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new System.Collections.Generic.List<string> { "card" },
                    LineItems = new System.Collections.Generic.List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(amount * 100),
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = $"{minutes} Session Minutes",
                                    Description = $"Purchase {minutes} minutes for session meetings"
                                },
                            },
                            Quantity = 1,
                        },
                    },
                    Mode = "payment",
                    SuccessUrl = $"{_appUrl}/payment-success?sessionId={{CHECKOUT_SESSION_ID}}&minutes={minutes}",
                    CancelUrl = $"{_appUrl}/payment-cancel",
                    CustomerEmail = user.Email,
                    Metadata = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "userId", userId },
                        { "minutes", minutes.ToString() },
                        { "planType", "minutes" }
                    }
                };

                var session = await new SessionService().CreateAsync(options);

                var payment = new Payment
                {
                    UserId = userId,
                    MinutesPurchased = minutes,
                    Amount = amount,
                    PaymentType = PaymentType.MinutesPurchase,
                    Status = PaymentStatus.Pending,
                    StripePaymentIntentId = session.Id,
                    CreatedOn = DateTime.UtcNow
                };

                await _context.Payments.AddAsync(payment);
                await _context.SaveChangesAsync();

                return new CreatePaymentIntentResponseDto
                {
                    CheckoutUrl = session.Url,
                    PublishableKey = _stripePublishableKey,
                    Amount = amount,
                    Currency = "usd"
                };
            }
            catch (StripeException ex)
            {
                throw new Exception($"Stripe error: {ex.Message}");
            }
        }

        public async Task<bool> HandlePaymentSuccessAsync(string sessionId, string authenticatedUserId)
        {
            try
            {
                var service = new SessionService();
                var session = await service.GetAsync(sessionId);

                if (session == null || session.PaymentStatus != "paid")
                    return false;



              

                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.StripePaymentIntentId == sessionId);

                if (payment == null)
                    return false;

                // Ownership check: verify the authenticated user owns this payment
                if (payment.UserId != authenticatedUserId)
                    return false;

                // Idempotency check: if already completed, return success without double-crediting
                if (payment.Status == PaymentStatus.Completed)
                    return true;

                payment.Status = PaymentStatus.Completed;
                payment.CompletedOn = DateTime.UtcNow;

                if (payment.PaymentType == PaymentType.Subscription)
                {
                    await ProcessSubscriptionAsync(payment.UserId, payment.SubscriptionPlanId.Value, sessionId);
                }
                else if (payment.PaymentType == PaymentType.MinutesPurchase)
                {
                    await ProcessMinutesPurchaseAsync(payment.UserId, payment.MinutesPurchased.Value, payment.Amount);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing payment success: {ex.Message}");
            }
        }

        public async Task<SubscriptionPlan> GetSubscriptionPlanAsync(int planId)
        {
            return await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == planId && p.IsActive);
        }

        public async Task<bool> ProcessSubscriptionAsync(string userId, int planId, string sessionId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            var plan = await GetSubscriptionPlanAsync(planId);
            if (plan == null)
                return false;

            var endDate = DateTime.UtcNow.AddDays(plan.DurationInDays);

            var subscription = new DAL.Entities.Subscription
            {
                UserId = userId,
                SubscriptionPlanId = planId,
                StartDate = DateTime.UtcNow,
                EndDate = endDate,
                Status = SubscriptionStatus.Active,
                StripeSubscriptionId =sessionId,
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            user.SubscriptionEndDate = endDate;
            user.SessionMinutes += plan.SessionMinutes;
            user.IsPaidUser = true;

            await _context.Subscriptions.AddAsync(subscription);
            _context.Users.Update(user);

            // Assign PaidUser role
            await _userManager.AddToRoleAsync(user, "PaidUser");

            return true;
        }

        public async Task<bool> ProcessMinutesPurchaseAsync(string userId, int minutes, decimal amount)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.SessionMinutes += minutes;
            _context.Users.Update(user);

            return true;
        }

    }
}
