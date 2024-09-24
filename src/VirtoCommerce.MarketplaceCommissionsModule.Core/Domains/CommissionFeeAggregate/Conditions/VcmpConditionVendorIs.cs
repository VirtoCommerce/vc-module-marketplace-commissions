using System.Collections.Generic;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Domains
{
    public class VcmpConditionVendorIs : ConditionTree
    {
        public ICollection<string> VendorIds { get; set; } = new List<string>();

        public override bool IsSatisfiedBy(IEvaluationContext context)
        {
            return context is CommissionFeeEvaluationContext commissionFeeContext
                && VendorIds.Contains(commissionFeeContext.Entry.SellerId);
        }
    }
}
