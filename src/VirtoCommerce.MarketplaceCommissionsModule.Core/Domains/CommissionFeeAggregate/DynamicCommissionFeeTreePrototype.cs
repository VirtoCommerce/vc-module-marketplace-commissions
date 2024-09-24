using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Domains
{
    public class DynamicCommissionFeeTreePrototype : ConditionTree
    {
        public DynamicCommissionFeeTreePrototype()
        {
            WithAvailConditions(new VendorCommissionCondition()
                .WithAvailConditions(
                    new VcmpConditionVendorIs(),
                    new UserGroupsContainsCondition()
                ),
                new BlockCommissionCondition()
                .WithAvailConditions(
                    new VcmpConditionCategoryIs(),
                    new VcmpConditionProductIs()
                )
            );
            Children = AvailableChildren;
        }
    }
}
