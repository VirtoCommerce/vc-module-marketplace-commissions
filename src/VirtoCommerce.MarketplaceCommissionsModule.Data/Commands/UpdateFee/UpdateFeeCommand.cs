using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Commands
{
    public class UpdateFeeCommand : ICommand<CommissionFee>
    {
        public CommissionFeeDetails FeeDetails { get; set; }
    }
}
