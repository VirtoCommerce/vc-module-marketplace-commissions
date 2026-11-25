using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Data.BackgroundJobs;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Services;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.IntegrationEventHandlers
{
    public class CustomerOrderCreatedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly ICategoryService _categoryService;
        private readonly ICustomerOrderService _customerOrderService;
        private readonly ICommissionFeeEvaluator _commissionFeeEvaluator;

        public CustomerOrderCreatedEventHandler(
            ICategoryService categoryService,
            ICustomerOrderService customerOrderService,
            ICommissionFeeEvaluator commissionFeeEvaluator
            )
        {
            _categoryService = categoryService;
            _customerOrderService = customerOrderService;
            _commissionFeeEvaluator = commissionFeeEvaluator;
        }

        public virtual Task Handle(OrderChangedEvent message)
        {
            var addedOrders = message.ChangedEntries.Where(x => x.EntryState == EntryState.Added).Select(e => e.NewEntry).ToArray();

            var payload = AbstractTypeFactory<CommissionFeesJobPayload>.TryCreateInstance();
            payload.OrderIds = addedOrders.Select(x => x.Id).ToArray();

            // The static facade, not an injected IBackgroundJob: RegisterEventHandler resolves this handler once from the
            // root provider and keeps it for the process lifetime, so it must not capture a scoped dependency.
            return BackgroundJob.Enqueue<CommissionFeesJobHandler>(payload);
        }

        // Runs without the per-order lock when called directly; CommissionFeesJobHandler takes it.
        // Kept public: background jobs enqueued by an earlier version reference this method by name.
        public virtual void HandleOrderChangesInBackground(string[] orderIds)
        {
            foreach (var orderId in orderIds)
            {
                HandleOrderChanges(orderId);
            }
        }

        protected virtual void HandleOrderChanges(string customerOrderId)
        {
            HandleOrderChangesAsync(customerOrderId).GetAwaiter().GetResult();
        }

        protected virtual async Task HandleOrderChangesAsync(string customerOrderId)
        {
            var order = await _customerOrderService.GetByIdAsync(customerOrderId);
            if (order != null)
            {
                await _commissionFeeEvaluator.EvalAndApplyFees(order, _categoryService);

                using (EventSuppressor.SuppressEvents())
                {
                    await _customerOrderService.SaveChangesAsync([order]);
                }
            }
        }

    }
}
