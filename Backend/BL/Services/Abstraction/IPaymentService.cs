using BL.DTOs;
using DAL.Entities;


namespace BL.Services.Abstraction
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
}
