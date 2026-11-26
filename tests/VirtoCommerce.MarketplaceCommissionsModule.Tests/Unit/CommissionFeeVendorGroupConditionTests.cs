using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Services;
using VirtoCommerce.MarketplaceVendorModule.Data.Integrations;
using Xunit;
using VCMP = VirtoCommerce.MarketplaceVendorModule.Core.Domains;

namespace VirtoCommerce.MarketplaceCommissionsModule.Tests.Unit
{
    // VM-1862: a dynamic commission with "Vendor matching ... user group contains <group>" must apply
    // to the products of a vendor in that group
    public class CommissionFeeVendorGroupConditionTests
    {
        private const string ProductId = "TestProductId";

        [Theory]
        [InlineData("Gold", "Dyn_VendorGroup_Gold")]
        [InlineData("Silver", null)]
        public async Task Dynamic_fee_with_vendor_group_condition_applies_to_vendors_of_that_group(string sellerGroup, string expectedFeeId)
        {
            // Arrange
            // the vendor's groups are the groups of its organization (Seller.Groups => Member.Groups)
            var seller = new VCMP.Seller("TestSellerId", "TestSellerName")
            {
                Member = new Organization { Groups = new List<string> { sellerGroup } },
            };

            var groupFee = new DynamicCommissionFee
            {
                Id = "Dyn_VendorGroup_Gold",
                Type = CommissionFeeType.Dynamic,
                Priority = 1,
                ExpressionTree = new DynamicCommissionFeeTree
                {
                    Children = new List<IConditionTree>
                    {
                        new VendorCommissionCondition
                        {
                            All = true,
                            Children = new List<IConditionTree> { new UserGroupsContainsCondition { Group = "Gold" } },
                        },
                    },
                },
            };

            var sellerResolver = new Mock<ISellerResolver>();
            sellerResolver.Setup(x => x.ResolveByProductIds(It.IsAny<string[]>()))
                .ReturnsAsync(new Dictionary<string, VCMP.Seller> { { ProductId, seller } });
            var feeResolver = new Mock<ICommissionFeeResolver>();
            feeResolver.Setup(x => x.ResolveBySellerIds(It.IsAny<IList<string>>()))
                .ReturnsAsync(new Dictionary<string, CommissionFee>());
            var searchService = new Mock<ICommissionFeeSearchService>();
            searchService.Setup(x => x.SearchAsync(It.IsAny<SearchCommissionFeesQuery>(), It.IsAny<bool>()))
                .ReturnsAsync(new SearchCommissionFeesResult { Results = new List<CommissionFee> { groupFee } });

            var sut = new CommissionFeeEvaluator(searchService.Object, new Mock<ISellerCommissionCrudService>().Object,
                sellerResolver.Object, feeResolver.Object, new Mock<ILogger<CommissionFeeEvaluator>>().Object);

            var context = new CommissionFeeEvaluationContext
            {
                AllEntries = [new EntryFee { EntryId = "TestEntry", EntryType = "LineItem", ProductId = ProductId, Quantity = 1 }],
            };

            // Act
            var result = await sut.EvaluateFeeAsync(context);

            // Assert
            (result.Single().CommissionFee?.Id).Should().Be(expectedFeeId);
        }
    }
}
