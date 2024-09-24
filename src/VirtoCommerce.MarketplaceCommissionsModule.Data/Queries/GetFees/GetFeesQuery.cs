using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Queries
{
    public class GetFeesQuery : IQuery<CommissionFee[]>
    {
        public string[] Ids { get; set; }
    }
}
