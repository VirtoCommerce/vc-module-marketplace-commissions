using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Commands;
using VirtoCommerce.MarketplaceVendorModule.Core.Domains;
using VirtoCommerce.MarketplaceVendorModule.Data.Queries;
using Xunit;

namespace VirtoCommerce.MarketplaceCommissionsModule.Tests.Unit
{
    public class BulkUpdateSellerCommissionsTests
    {
        private const string SourceFeeId = "source-fee";
        private const string TargetFeeId = "target-fee";
        private const string DynamicFeeId = "dynamic-fee";

        [Fact]
        public async Task Handle_SelectedSellers_AssignsThemToTarget()
        {
            var context = new TestContext("seller1", "seller2");

            var result = await context.Handle(new BulkUpdateSellerCommissionsCommand
            {
                CommissionFeeId = SourceFeeId,
                TargetCommissionFeeId = TargetFeeId,
                SellerIds = ["seller1", "seller2"],
            });

            Assert.Equal(2, result.UpdatedCount);
            Assert.All(context.Saved, x => Assert.Equal(TargetFeeId, x.CommissionFeeId));
            Assert.Equal(["seller1", "seller2"], context.Saved.Select(x => x.SellerId).Order());
            var query = Assert.Single(context.Queries);
            Assert.Equal(SourceFeeId, query.CommissionFeeId);
            Assert.Equal(["seller1", "seller2"], query.SellerIds);
            Assert.Null(query.Keyword);
        }

        [Fact]
        public async Task Handle_AllSellers_SearchesByKeywordAndSkipsExcluded()
        {
            var context = new TestContext("seller1", "seller2", "seller3");

            var result = await context.Handle(new BulkUpdateSellerCommissionsCommand
            {
                CommissionFeeId = SourceFeeId,
                TargetCommissionFeeId = TargetFeeId,
                AllSellers = true,
                Keyword = "Acme",
                SellerIds = ["ignored"],
                ExcludedSellerIds = ["seller2"],
            });

            Assert.Equal(2, result.UpdatedCount);
            Assert.Equal(["seller1", "seller3"], context.Saved.Select(x => x.SellerId).Order());
            Assert.Equal(["Acme"], context.SellerKeywords);
            var query = Assert.Single(context.Queries);
            Assert.Equal(["seller1", "seller2", "seller3"], query.SellerIds);
            Assert.Null(query.Keyword);
        }

        [Fact]
        public async Task Handle_AllSellers_KeywordMatchesNoVendor_UpdatesNothing()
        {
            var context = new TestContext("seller1");

            var result = await context.Handle(new BulkUpdateSellerCommissionsCommand
            {
                CommissionFeeId = SourceFeeId,
                TargetCommissionFeeId = TargetFeeId,
                AllSellers = true,
                Keyword = "nobody",
            });

            Assert.Equal(0, result.UpdatedCount);
            Assert.Empty(context.Queries);
            Assert.Empty(context.SaveBatches);
        }

        [Fact]
        public async Task Handle_ManySellers_ReadsAllPagesBeforeSavingInBatches()
        {
            var sellerIds = Enumerable.Range(1, 230).Select(x => $"seller{x}").ToArray();
            var context = new TestContext(sellerIds);

            var result = await context.Handle(new BulkUpdateSellerCommissionsCommand
            {
                CommissionFeeId = SourceFeeId,
                TargetCommissionFeeId = TargetFeeId,
                AllSellers = true,
            });

            Assert.Equal(230, result.UpdatedCount);
            Assert.Empty(context.SellerKeywords);
            Assert.All(context.Queries, x => Assert.Null(x.SellerIds));
            Assert.Equal([0, 100, 200], context.Queries.Select(x => x.Skip));
            Assert.Equal([50, 50, 50, 50, 30], context.SaveBatches.Select(x => x.Count));
            Assert.True(context.AllPagesReadBeforeFirstSave);
        }

        [Fact]
        public async Task Handle_NoSelection_UpdatesNothing()
        {
            var context = new TestContext("seller1");

            var result = await context.Handle(new BulkUpdateSellerCommissionsCommand
            {
                CommissionFeeId = SourceFeeId,
                TargetCommissionFeeId = TargetFeeId,
            });

            Assert.Equal(0, result.UpdatedCount);
            Assert.Empty(context.Queries);
            Assert.Empty(context.SaveBatches);
        }

        [Theory]
        [InlineData(null, TargetFeeId)]
        [InlineData(SourceFeeId, null)]
        [InlineData(SourceFeeId, SourceFeeId)]
        [InlineData(SourceFeeId, "unknown-fee")]
        [InlineData(SourceFeeId, DynamicFeeId)]
        public async Task Handle_InvalidCommissionFees_Throws(string sourceId, string targetId)
        {
            var context = new TestContext("seller1");

            await Assert.ThrowsAsync<OperationCanceledException>(() => context.Handle(new BulkUpdateSellerCommissionsCommand
            {
                CommissionFeeId = sourceId,
                TargetCommissionFeeId = targetId,
                SellerIds = ["seller1"],
            }));

            Assert.Empty(context.SaveBatches);
        }

        private sealed class TestContext
        {
            private readonly List<SellerCommission> _assignments;
            private readonly Mock<ICommissionFeeService> _feeService = new();
            private readonly Mock<ISellerCommissionSearchService> _searchService = new();
            private readonly Mock<ISellerCommissionCrudService> _crudService = new();
            private readonly Mock<ISellersSearchService> _sellersSearchService = new();

            public List<SearchSellerCommissionsQuery> Queries { get; } = [];
            public List<string> SellerKeywords { get; } = [];
            public List<IList<SellerCommission>> SaveBatches { get; } = [];
            public IEnumerable<SellerCommission> Saved => SaveBatches.SelectMany(x => x);
            public bool AllPagesReadBeforeFirstSave { get; private set; } = true;

            public TestContext(params string[] sellerIds)
            {
                _assignments = sellerIds.Select(x => new SellerCommission { Id = $"sc-{x}", SellerId = x, CommissionFeeId = SourceFeeId }).ToList();

                _searchService
                    .Setup(x => x.SearchAsync(It.IsAny<SearchSellerCommissionsQuery>(), It.IsAny<bool>()))
                    .ReturnsAsync((SearchSellerCommissionsQuery q, bool _) =>
                    {
                        // a copy: SearchAllAsync moves Skip on the same criteria object
                        Queries.Add(new SearchSellerCommissionsQuery { CommissionFeeId = q.CommissionFeeId, SellerIds = q.SellerIds, Keyword = q.Keyword, Skip = q.Skip, Take = q.Take });
                        if (SaveBatches.Count > 0)
                        {
                            AllPagesReadBeforeFirstSave = false;
                        }

                        var matching = _assignments.Where(a => a.CommissionFeeId == q.CommissionFeeId && (q.SellerIds == null || q.SellerIds.Contains(a.SellerId))).ToList();
                        return new SearchSellerCommissionsResult { TotalCount = matching.Count, Results = matching.Skip(q.Skip).Take(q.Take).ToList() };
                    });

                // the keyword "Acme" matches every vendor, any other keyword matches none
                _sellersSearchService
                    .Setup(x => x.SearchAsync(It.IsAny<SearchSellersQuery>(), It.IsAny<bool>()))
                    .ReturnsAsync((SearchSellersQuery q, bool _) =>
                    {
                        if (q.Skip == 0)
                        {
                            SellerKeywords.Add(q.Keyword);
                        }

                        var matching = q.Keyword == "Acme" ? sellerIds.Select(x => new Seller { Id = x }).ToList() : [];
                        return new SearchSellersResult { TotalCount = matching.Count, Results = matching.Skip(q.Skip).Take(q.Take).ToList() };
                    });

                var fees = new Dictionary<string, CommissionFee>
                {
                    [SourceFeeId] = new() { Id = SourceFeeId, Type = CommissionFeeType.Static },
                    [TargetFeeId] = new() { Id = TargetFeeId, Type = CommissionFeeType.Static },
                    [DynamicFeeId] = new() { Id = DynamicFeeId, Type = CommissionFeeType.Dynamic },
                };
                _feeService
                    .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync((IList<string> ids, string _, bool _) => ids.Where(fees.ContainsKey).Select(id => fees[id]).ToList());

                _crudService
                    .Setup(x => x.SaveChangesAsync(It.IsAny<IList<SellerCommission>>()))
                    .Callback<IList<SellerCommission>>(x => SaveBatches.Add(x.ToList()))
                    .Returns(Task.CompletedTask);
            }

            public Task<Core.Models.BulkUpdateSellerCommissionsResult> Handle(BulkUpdateSellerCommissionsCommand command)
            {
                return new BulkUpdateSellerCommissionsCommandHandler(_feeService.Object, _searchService.Object, _crudService.Object, _sellersSearchService.Object).Handle(command, CancellationToken.None);
            }
        }
    }
}
