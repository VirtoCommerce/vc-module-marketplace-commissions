using VirtoCommerce.MarketplaceVendorModule.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Domains
{
    public class SellerCommission : AggregateRoot
    {
        public string SellerId { get; set; }
        public string CommissionFeeId { get; set; }
    }
}
