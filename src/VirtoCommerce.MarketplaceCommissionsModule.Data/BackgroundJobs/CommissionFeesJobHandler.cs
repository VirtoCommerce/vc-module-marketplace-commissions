using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Data.IntegrationEventHandlers;
using VirtoCommerce.Platform.Core.DistributedLock;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.BackgroundJobs
{
    /// <summary>
    /// Applies commission fees to new customer orders, off the request thread.
    /// </summary>
    /// <remarks>
    /// Delegates to <see cref="CustomerOrderCreatedEventHandler"/>, which still owns the logic and stays callable by
    /// background jobs enqueued by an earlier version that reference its method by name.
    /// </remarks>
    public class CommissionFeesJobHandler : IBackgroundJobHandler<CommissionFeesJobPayload>
    {
        private readonly CustomerOrderCreatedEventHandler _eventHandler;
        private readonly IDistributedLock _distributedLock;

        public CommissionFeesJobHandler(CustomerOrderCreatedEventHandler eventHandler, IDistributedLock distributedLock)
        {
            _eventHandler = eventHandler;
            _distributedLock = distributedLock;
        }

        public virtual async Task Execute(CommissionFeesJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            foreach (var orderId in payload.OrderIds ?? [])
            {
                // Reads the order, adds its fees and saves it: one run per order at a time, so two runs cannot both add
                // fees (replaces Hangfire's DisableConcurrentExecution, which serialized all orders).
                await _distributedLock.ExecuteAsync($"vcmp:commission-fees:{orderId}", _ =>
                {
                    _eventHandler.HandleOrderChangesInBackground([orderId]);
                    return Task.CompletedTask;
                }, Timeout.InfiniteTimeSpan, cancellationToken);
            }
        }
    }
}
