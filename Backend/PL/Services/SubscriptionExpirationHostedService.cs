
using BL.Services.Abstraction;

namespace PL.Services
{
    public class SubscriptionExpirationHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SubscriptionExpirationHostedService> _logger;
        private Timer _timer;

        public SubscriptionExpirationHostedService(IServiceProvider serviceProvider, ILogger<SubscriptionExpirationHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(async _ => await CheckExpiredSubscriptions(), null, TimeSpan.Zero, TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        private async Task CheckExpiredSubscriptions()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
                    await subscriptionService.CheckAndUpdateExpiredSubscriptionsAsync();
                    _logger.LogInformation("Subscription expiration check completed at {time}", DateTime.UtcNow);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking subscription expiration");
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            return base.StopAsync(cancellationToken);
        }
    }
}
