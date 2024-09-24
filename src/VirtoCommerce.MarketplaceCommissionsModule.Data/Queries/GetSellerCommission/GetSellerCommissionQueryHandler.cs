using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Queries
{
    public class GetSellerCommissionQueryHandler : IQueryHandler<GetSellerCommissionQuery, CommissionFee>
    {
        private readonly ICommissionFeeService _commissionFeeService;

        public GetSellerCommissionQueryHandler(ICommissionFeeService _commissionFeeService)
        {
            this._commissionFeeService = _commissionFeeService;
        }

        public virtual async Task<CommissionFee> Handle(GetSellerCommissionQuery request, CancellationToken cancellationToken)
        {
            var commissionFee = await _commissionFeeService.GetCommissionBySellerId(request.SellerId);
            return commissionFee;
        }
    }
}
