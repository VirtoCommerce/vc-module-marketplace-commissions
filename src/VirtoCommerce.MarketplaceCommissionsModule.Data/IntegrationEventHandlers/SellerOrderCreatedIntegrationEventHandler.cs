using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Services;
using VirtoCommerce.MarketplaceVendorModule.Core.IntegrationEvents;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.IntegrationEventHandlers
{
    public class SellerOrderCreatedIntegrationEventHandler : IEventHandler<SellerOrderCreatedIntegrationEvent>
    {
        private readonly ICustomerOrderService _customerOrderService;
        private readonly ICommissionFeeEvaluator _commissionFeeEvaluator;
        private readonly ICategoryService _categoryService;

        public SellerOrderCreatedIntegrationEventHandler(
            ICustomerOrderService customerOrderService,
            ICommissionFeeEvaluator commissionFeeEvaluator,
            ICategoryService categoryService
            )
        {
            _customerOrderService = customerOrderService;
            _commissionFeeEvaluator = commissionFeeEvaluator;
            _categoryService = categoryService;
        }

        public virtual async Task Handle(SellerOrderCreatedIntegrationEvent message)
        {
            var order = await _customerOrderService.GetByIdAsync(message.SellerOrderId);
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
