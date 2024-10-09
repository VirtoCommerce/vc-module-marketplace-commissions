using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;
using VirtoCommerce.MarketplaceVendorModule.Core.IntegrationEvents;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.IntegrationEventHandlers
{
    public class SellerCreatedIntegrationEventHandler : IEventHandler<SellerCreatedIntegrationEvent>
    {
        private readonly ISellerCommissionCrudService _sellerCommissionCrudService;
        private readonly ICommissionFeeService _commissionFeeService;

        public SellerCreatedIntegrationEventHandler(
            ISellerCommissionCrudService sellerCommissionCrudService,
            ICommissionFeeService commissionFeeService
            )
        {
            _sellerCommissionCrudService = sellerCommissionCrudService;
            _commissionFeeService = commissionFeeService;
        }

        public virtual async Task Handle(SellerCreatedIntegrationEvent message)
        {
            var sellerId = message.SellerId;
            if (!string.IsNullOrEmpty(sellerId))
            {
                var existedSellerCommission = await _sellerCommissionCrudService.GetSellerCommission(sellerId);
                var defaultCommission = await _commissionFeeService.GetDefaultCommissionFee();

                if (existedSellerCommission == null && defaultCommission != null)
                {
                    var sellerCommission = ExType<SellerCommission>.New();
                    sellerCommission.SellerId = sellerId;
                    sellerCommission.CommissionFeeId = defaultCommission.Id;

                    await _sellerCommissionCrudService.SaveChangesAsync([sellerCommission]);
                }
            }
        }
    }
}
