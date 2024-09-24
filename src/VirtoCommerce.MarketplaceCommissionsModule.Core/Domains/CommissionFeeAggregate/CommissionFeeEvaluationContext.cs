using VirtoCommerce.CoreModule.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Domains
{
    public class CommissionFeeEvaluationContext : EvaluationContextBase
    {
        public EntryFee Entry { get; set; }
        public EntryFee[] AllEntries { get; set; }
    }
}
