using VirtoCommerce.MarketplaceVendorModule.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Commands
{
    public class DeleteFeeCommand : ICommand
    {
        public string Id { get; set; }
    }
}
