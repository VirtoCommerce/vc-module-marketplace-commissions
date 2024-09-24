using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Domains
{
    public class VcmpConditionCategoryIs : ConditionTree
    {
        public ICollection<string> ExcludingCategories { get; set; } = new List<string>();
        public ICollection<string> IncludingCategories { get; set; } = new List<string>();
        public ICollection<string> ExcludingProducts { get; set; } = new List<string>();

        public override bool IsSatisfiedBy(IEvaluationContext context)
        {
            var result = false;
            if (context is CommissionFeeEvaluationContext commissionFeeContext)
            {
                result = new EntryFee[] { commissionFeeContext.Entry }
                    .InCategories(IncludingCategories)
                    .ExcludeCategories(ExcludingCategories)
                    .ExcludeProducts(ExcludingProducts)
                    .Any();
            }

            return result;
        }
    }
}
