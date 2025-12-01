using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Domains
{
    public class DynamicCommissionFeeTreePrototype : ConditionTree
    {
        public DynamicCommissionFeeTreePrototype()
        {
            WithAvailableChildren(new VendorCommissionCondition()
                .WithAvailableChildren(
                    new VcmpConditionVendorIs(),
                    new UserGroupsContainsCondition()
                ),
                new BlockCommissionCondition()
                .WithAvailableChildren(
                    new VcmpConditionCategoryIs(),
                    new VcmpConditionProductIs()
                )
            );
            Children = AvailableChildren;
        }
    }
}
