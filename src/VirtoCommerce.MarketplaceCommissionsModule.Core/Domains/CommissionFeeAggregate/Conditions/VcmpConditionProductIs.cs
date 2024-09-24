using System.Collections.Generic;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Domains
{
    public class VcmpConditionProductIs : ConditionTree
    {
        public ICollection<string> IncludingProducts { get; set; } = new List<string>();

        public override bool IsSatisfiedBy(IEvaluationContext context)
        {
            var result = false;
            if (context is CommissionFeeEvaluationContext commissionFeeContext)
            {
                result = commissionFeeContext.Entry?.IsEntryInProducts(IncludingProducts) ?? false;
            }
            return result;
        }
    }
}
