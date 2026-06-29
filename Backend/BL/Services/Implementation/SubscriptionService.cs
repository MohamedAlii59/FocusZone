
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DAL.Entities;
using DAL.Database;
using BL.DTOs;
using BL.Services.Abstraction;

namespace BL.Services.Implementation
{
   

    public class SubscriptionService : ISubscriptionService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public SubscriptionService(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IList<SubscriptionPlanDto>> GetActiveSubscriptionPlansAsync()
        {
            var plans = await _context.SubscriptionPlans
                .Where(p => p.IsActive)
                .Select(p => new SubscriptionPlanDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    DurationInDays = p.DurationInDays,
                    SessionMinutes = p.SessionMinutes,
                    PlanType = p.PlanType.ToString(),
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return plans;
        }

        public async Task<UserSubscriptionResponseDto> GetUserSubscriptionAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.Subscriptions)
                .ThenInclude(s => s.SubscriptionPlan)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            var activeSubscription = user.Subscriptions
                .Where(s => s.IsActive && s.Status == SubscriptionStatus.Active && s.EndDate > DateTime.UtcNow)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefault();

            return new UserSubscriptionResponseDto
            {
                ActiveSubscriptionId = activeSubscription?.Id,
                ActiveSubscriptionName = activeSubscription?.SubscriptionPlan?.Name,
                SubscriptionEndDate = user.SubscriptionEndDate,
                SessionMinutes = user.SessionMinutes,
                IsPaidUser = user.IsPaidUser
            };
        }

        public async Task<bool> CheckAndUpdateExpiredSubscriptionsAsync()
        {
            var now = DateTime.UtcNow;
            var expiredSubscriptions = await _context.Subscriptions
                .Where(s => s.IsActive && s.EndDate <= now && s.Status == SubscriptionStatus.Active)
                .Include(s => s.User)
                .ToListAsync();

            foreach (var subscription in expiredSubscriptions)
            {
                subscription.Status = SubscriptionStatus.Expired;
                subscription.IsActive = false;

                var user = subscription.User;

                // Check if user has any other active subscriptions
                var hasOtherActiveSubscription = await _context.Subscriptions
                    .AnyAsync(s => s.UserId == user.Id && s.Id != subscription.Id && s.IsActive && s.Status == SubscriptionStatus.Active && s.EndDate > now);

                if (!hasOtherActiveSubscription)
                {
                    user.IsPaidUser = false;
                    user.SubscriptionEndDate = null;

                    // Remove "PaidUser" role
                    await _userManager.RemoveFromRoleAsync(user, "PaidUser");
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Cancel the user's active subscription, immediately revoking paid status.
        /// 
        /// NOTE: Subscriptions are one-time purchases (not Stripe recurring billing).
        /// Cancellation means "revoke paid access early" and has no refund logic (out of scope).
        /// No Stripe-side subscription object exists to cancel via API.
        /// </summary>
        public async Task<bool> CancelSubscriptionAsync(string userId)
        {
            var activeSubscription = await _context.Subscriptions
                .Where(s => s.UserId == userId && s.IsActive && s.Status == SubscriptionStatus.Active)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            if (activeSubscription == null)
                return false;

            activeSubscription.Status = SubscriptionStatus.Cancelled;
            activeSubscription.IsActive = false;

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsPaidUser = false;
                user.SubscriptionEndDate = null;
                await _userManager.RemoveFromRoleAsync(user, "PaidUser");
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<PaymentDto>> GetUserPaymentsAsync(string userId)
        {
            var payments = await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedOn)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Amount = p.Amount,
                    PaymentType = p.PaymentType.ToString(),
                    Status = p.Status.ToString(),
                    CreatedOn = p.CreatedOn,
                    CompletedOn = p.CompletedOn
                })
                .ToListAsync();

            return payments;
        }
    }
}
