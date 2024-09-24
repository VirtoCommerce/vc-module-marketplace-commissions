using VirtoCommerce.MarketplaceVendorModule.Core.Common;
using VirtoCommerce.MarketplaceVendorModule.Core.Domains;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Commands
{
    public class UpdateSellerCommissionCommand : ICommand, IHasSellerId
    {
        public string SellerId { get; set; }
        public string SellerName { get; set; }
        public string CommissionFeeId { get; set; }
    }
}
