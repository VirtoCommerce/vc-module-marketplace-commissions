using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Services;
using VirtoCommerce.MarketplaceVendorModule.Data.Integrations;
using VirtoCommerce.OrdersModule.Core.Model;
using Xunit;
using VCMP = VirtoCommerce.MarketplaceVendorModule.Core.Domains;

namespace VirtoCommerce.MarketplaceCommissionsModule.Tests.Unit
{
    public class CommissionFeeEvaluationTests
    {
        private readonly Mock<ISellerResolver> _sellerByProductIdResolver = new();
        private readonly Mock<ICommissionFeeResolver> _commissionFeeResolver = new();
        private readonly Mock<ICommissionFeeSearchService> _searchService = new();
        private readonly Mock<ISellerCommissionCrudService> _sellerCommissionCrudService = new();
        private readonly Mock<ILogger<CommissionFeeEvaluator>> _loggerMock = new();

        [Theory]
        [MemberData(nameof(Input))]
        public async Task Evaluate_static_and_mutiple_dynamics_fees_one_dynamic_fee_with_high_priority_is_returned(LineItem lineItem, string expectedComissionFeeId)
        {
            // Arrange
            var seller = new VCMP.Seller("TestSellerId", "TestSellerName");
            var commissionFee = new CommissionFee
            {
                Id = "TestStaticCommissionFee",
                Type = CommissionFeeType.Static,
                Fee = 10,
                CalculationType = FeeCalculationType.Percent
            };
            var sellerCommission = new SellerCommission() { SellerId = seller.Id, CommissionFeeId = commissionFee.Id };

            var dynamicCommissionFees = new SearchCommissionFeesResult
            {
                Results = new TestCommissionFee().ListTestDynamicCommissionFee
            };

            _sellerByProductIdResolver.Setup(a => a.ResolveByProductIds(It.IsAny<string[]>())).ReturnsAsync(new Dictionary<string, VCMP.Seller> { { lineItem.ProductId, seller } });
            _commissionFeeResolver.Setup(a => a.ResolveBySellerIds(It.IsAny<string[]>())).ReturnsAsync(new Dictionary<string, CommissionFee> { { lineItem.VendorId, commissionFee } });
            _searchService.Setup(a => a.SearchAsync(It.IsAny<SearchCommissionFeesQuery>(), It.IsAny<bool>())).ReturnsAsync(dynamicCommissionFees);
            _sellerCommissionCrudService.Setup(a => a.GetSellersCommissions(It.IsAny<IList<string>>())).ReturnsAsync([sellerCommission]);
            var sut = new CommissionFeeEvaluator(_searchService.Object, _sellerCommissionCrudService.Object, _sellerByProductIdResolver.Object, _commissionFeeResolver.Object, _loggerMock.Object);


            var order = new CustomerOrder
            {
                EmployeeId = "TestSellerId",
                Items = new List<LineItem> { lineItem }
            };

            var context = new CommissionFeeEvaluationContext
            {
                AllEntries = order.Items.Select(x => new EntryFee
                {
                    CategoryId = x.CategoryId,
                    Outline = x.CategoryId,
                    EntryId = x.Id,
                    EntryType = nameof(LineItem),
                    Price = x.ExtendedPrice,
                    PriceWithTax = x.ExtendedPriceWithTax,
                    ProductId = x.ProductId,
                    Quantity = x.Quantity
                }).ToArray()
            };

            // Act
            var entryFee = await sut.EvaluateFeeAsync(context);

            // Assertion
            entryFee.Single().CommissionFee.Id.Should().Be(expectedComissionFeeId);
        }

        public static TheoryData<LineItem, string> Input()
        {
            return new TheoryData<LineItem, string>()
            {
                {
                    new LineItem
                    {
                        Id = "TestId1",
                        CategoryId = "TestCategoryId",
                        ProductId = "TestProductId1",
                        VendorId = "TestSellerId"
                    },
                    "Dyn_CategoryIs_TestCategoryId_4"
                },
                {
                    new LineItem
                    {
                        Id = "TestId2",
                        ProductId = "TestProductId",
                        VendorId = "TestSellerId"
                    },
                    "Dyn_ProductIs_TestProductId_5"
                },
                {
                    new LineItem
                    {
                        Id = "TestId3",
                        CategoryId = "NoDynamicCondition",
                        ProductId = "TestProductId3",
                        VendorId = "TestSellerId"
                    },
                    "TestStaticCommissionFee"
                },
                {
                    new LineItem
                    {
                        Id = "TestId4",
                        CategoryId = "TestCategoryId",
                        ProductId = "TestProductId4",
                        VendorId = "TestSellerId"
                    },
                    "Dyn_CategoryIs_TestCategoryId_2"
                },
            };
        }
    }
}
