using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search
{
    public class SearchCommissionFeesQuery : SearchCriteriaBase, IQuery<SearchCommissionFeesResult>
    {
        public CommissionFeeType? Type { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsActive { get; set; }
    }
}
