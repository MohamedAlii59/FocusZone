using BL.DTOs;


namespace BL.Services.Abstraction
{
    public interface ISubscriptionService
    {
        Task<IList<SubscriptionPlanDto>> GetActiveSubscriptionPlansAsync();
        Task<UserSubscriptionResponseDto> GetUserSubscriptionAsync(string userId);
        Task<bool> CheckAndUpdateExpiredSubscriptionsAsync();
        Task<bool> CancelSubscriptionAsync(string userId);
        Task<IList<PaymentDto>> GetUserPaymentsAsync(string userId);
    }
}
