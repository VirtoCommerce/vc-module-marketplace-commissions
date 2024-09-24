using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Services
{
    public class CommissionFeeResolver : ICommissionFeeResolver
    {
        private readonly ISellerCommissionCrudService _sellerCommissionCrudService;
        private readonly ICommissionFeeService _commissionFeeService;

        public CommissionFeeResolver(
            ISellerCommissionCrudService sellerCommissionCrudService,
            ICommissionFeeService commissionFeeService
            )
        {
            _sellerCommissionCrudService = sellerCommissionCrudService;
            _commissionFeeService = commissionFeeService;
        }
        public virtual async Task<IDictionary<string, CommissionFee>> ResolveBySellerIds(string[] sellerIds)
        {
            var result = new Dictionary<string, CommissionFee>();

            if (sellerIds != null && sellerIds.Any())
            {
                var sellersCommissions = (await _sellerCommissionCrudService.GetSellersCommissions(sellerIds)).ToList();
                var commissionFeeIds = sellersCommissions.Select(x => x.CommissionFeeId).Distinct().ToList();
                var commissionFeesMap = (await _commissionFeeService.GetAsync(commissionFeeIds)).ToDictionary(x => x.Id, x => x);

                foreach (var sellerCommission in sellersCommissions)
                {
                    result.TryAdd(sellerCommission.SellerId, commissionFeesMap[sellerCommission.CommissionFeeId]);
                }
            }

            return result;
        }
    }
}
