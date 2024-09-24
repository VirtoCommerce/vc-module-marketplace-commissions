using VirtoCommerce.MarketplaceVendorModule.Core.Common;
using VirtoCommerce.MarketplaceVendorModule.Core.Domains;
using CommissionFee = VirtoCommerce.MarketplaceCommissionsModule.Core.Domains.CommissionFee;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Queries
{
    public class GetSellerCommissionQuery : IQuery<CommissionFee>, IHasSellerId
    {
        public string SellerId { get; set; }
        public string SellerName { get; set; }
    }
}
