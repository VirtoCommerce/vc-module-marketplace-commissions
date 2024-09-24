using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Queries
{
    public class GetFeesQueryHandler : IQueryHandler<GetFeesQuery, CommissionFee[]>
    {
        private readonly ICommissionFeeService _commissionFeeService;

        public GetFeesQueryHandler(ICommissionFeeService commissionFeeService)
        {
            _commissionFeeService = commissionFeeService;
        }

        public virtual async Task<CommissionFee[]> Handle(GetFeesQuery request, CancellationToken cancellationToken)
        {
            var result = await _commissionFeeService.GetAsync(request.Ids?.ToList());
            return result.ToArray();
        }
    }
}
