using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Domains
{
    public class DynamicCommissionFeeTree : BlockConditionAndOr
    {
        public DynamicCommissionFeeTree()
        {
            All = true;
        }
    }
}
