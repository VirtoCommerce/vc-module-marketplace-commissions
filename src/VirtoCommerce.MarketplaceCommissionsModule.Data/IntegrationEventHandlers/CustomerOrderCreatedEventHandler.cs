using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Services;
using VirtoCommerce.MarketplaceVendorModule.Data.BackgroundJobs;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.IntegrationEventHandlers
{
    public class CustomerOrderCreatedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly ICategoryService _categoryService;
        private readonly ICustomerOrderService _customerOrderService;
        private readonly ICommissionFeeEvaluator _commissionFeeEvaluator;
        private readonly IBackgroundJobExecutor _backgroundJobExecutor;

        public CustomerOrderCreatedEventHandler(
            ICategoryService categoryService,
            ICustomerOrderService customerOrderService,
            ICommissionFeeEvaluator commissionFeeEvaluator,
            IBackgroundJobExecutor backgroundJobExecutor
            )
        {
            _categoryService = categoryService;
            _customerOrderService = customerOrderService;
            _commissionFeeEvaluator = commissionFeeEvaluator;
            _backgroundJobExecutor = backgroundJobExecutor;
        }

        public virtual Task Handle(OrderChangedEvent message)
        {
            var addedOrders = message.ChangedEntries.Where(x => x.EntryState == EntryState.Added).Select(e => e.NewEntry).ToArray();
            _backgroundJobExecutor.Enqueue(() => HandleOrderChangesInBackground(addedOrders.Select(x => x.Id).ToArray()));
            return Task.CompletedTask;
        }

        [DisableConcurrentExecution(10)]
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
