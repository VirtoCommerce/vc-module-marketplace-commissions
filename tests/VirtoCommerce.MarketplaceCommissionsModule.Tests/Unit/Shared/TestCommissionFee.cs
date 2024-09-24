using System.Collections.Generic;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;


namespace VirtoCommerce.MarketplaceCommissionsModule.Tests.Unit
{
    public class TestCommissionFee
    {
        public List<CommissionFee> ListTestDynamicCommissionFee
        {
            get
            {
                return new List<CommissionFee>
                {
                    new DynamicCommissionFee
                    {
                        Id = "Dyn_CategoryIs_TestCategoryId_1",
                        Type = CommissionFeeType.Dynamic,
                        Priority = 1,
                        ExpressionTree = new DynamicCommissionFeeTree
                        {
                            Children = new List<IConditionTree>
                            {
                                new VcmpConditionCategoryIs
                                {
                                    IncludingCategories = new List<string> { "TestCategoryId" }
                                }
                            }
                        }
                    },
                    new DynamicCommissionFee
                    {
                        Id = "Dyn_CategoryIs_TestCategoryId_2",
                        Type = CommissionFeeType.Dynamic,
                        Priority = 2,
                        ExpressionTree = new DynamicCommissionFeeTree
                        {
                            Children = new List<IConditionTree>
                            {
                                new VcmpConditionCategoryIs
                                {
                                    IncludingCategories = new List<string> { "TestCategoryId" }
                                }
                            }
                        }
                    },
                    new DynamicCommissionFee
                    {
                        Id = "Dyn_ProductIs_TestProductId_1",
                        Type = CommissionFeeType.Dynamic,
                        Priority = 1,
                        ExpressionTree = new DynamicCommissionFeeTree
                        {
                            Children = new List<IConditionTree>
                            {
                                new VcmpConditionProductIs
                                {
                                     IncludingProducts = new List<string> { "TestProductId" }
                                }
                            }
                        }
                    },
                    new DynamicCommissionFee
                    {
                        Id = "Dyn_ProductIs_TestProductId_5",
                        Type = CommissionFeeType.Dynamic,
                        Priority = 5,
                        ExpressionTree = new DynamicCommissionFeeTree
                        {
                            Children = new List<IConditionTree>
                            {
                                new VcmpConditionProductIs
                                {
                                    IncludingProducts = new List<string> { "TestProductId" }
                                }
                            }
                        }
                    },
                    new DynamicCommissionFee
                    {
                        Id = "Dyn_CategoryIs_TestCategoryId_4",
                        Type = CommissionFeeType.Dynamic,
                        Priority = 4,
                        ExpressionTree = new DynamicCommissionFeeTree
                        {
                            Children = new List<IConditionTree>
                            {
                                new VcmpConditionCategoryIs
                                {
                                    IncludingCategories = new List<string> { "TestCategoryId" },
                                    ExcludingProducts = new List<string> { "TestProductId4" }
                                }
                            }
                        }
                    }
                };
            }
        }
    }
}
