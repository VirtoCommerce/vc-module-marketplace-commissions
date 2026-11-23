using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Models;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Queries;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Repositories;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Services;
using VirtoCommerce.MarketplaceVendorModule.Core.Domains;
using VirtoCommerce.MarketplaceVendorModule.Data.Queries;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using Xunit;

namespace VirtoCommerce.MarketplaceCommissionsModule.Tests.Unit
{
    public class SellerCommissionSearchTests
    {
        private const string StaticFeeId = "static-fee";
        private const string OtherFeeId = "other-fee";

        private static readonly List<SellerCommissionEntity> SellerCommissions =
        [
            NewSellerCommission("sc1", StaticFeeId, "seller1"),
            NewSellerCommission("sc2", StaticFeeId, "seller2"),
            NewSellerCommission("sc3", OtherFeeId, "seller3"),
        ];

        [Fact]
        public void BuildQuery_NoFilters_ReturnsAll()
        {
            var result = Search(new SearchSellerCommissionsQuery());

            Assert.Equal(["sc1", "sc2", "sc3"], result.Select(x => x.Id).Order());
        }

        [Fact]
        public void BuildQuery_ByCommissionFeeId_ReturnsOnlyItsVendors()
        {
            var result = Search(new SearchSellerCommissionsQuery { CommissionFeeId = StaticFeeId });

            Assert.Equal(["seller1", "seller2"], result.Select(x => x.SellerId).Order());
        }

        [Fact]
        public void BuildQuery_ByCommissionFeeIdAndSellerIds_ReturnsOnlyThoseVendors()
        {
            var result = Search(new SearchSellerCommissionsQuery { CommissionFeeId = StaticFeeId, SellerIds = ["seller2", "seller3"] });

            Assert.Equal(["seller2"], result.Select(x => x.SellerId));
        }

        [Fact]
        public void BuildQuery_UnknownCommissionFeeId_ReturnsNothing()
        {
            var result = Search(new SearchSellerCommissionsQuery { CommissionFeeId = "unknown" });

            Assert.Empty(result);
        }

        [Fact]
        public void BuildSortExpression_NoSort_NewestAssignmentFirst()
        {
            var service = new TestSellerCommissionSearchService(Mock.Of<ICommissionFeeRepository>);

            var sortInfo = Assert.Single(service.BuildSortExpressionPublic(new SearchSellerCommissionsQuery()));

            Assert.Equal(nameof(SellerCommissionEntity.CreatedDate), sortInfo.SortColumn);
            Assert.Equal(SortDirection.Descending, sortInfo.SortDirection);
        }

        [Fact]
        public async Task Handler_NoKeyword_PassesQueryToSearchService()
        {
            var query = new SearchSellerCommissionsQuery { CommissionFeeId = StaticFeeId, Take = 0 };
            var expected = new SearchSellerCommissionsResult { TotalCount = 2 };
            var searchService = new Mock<ISellerCommissionSearchService>();
            searchService.Setup(x => x.SearchAsync(query, It.IsAny<bool>())).ReturnsAsync(expected);
            var sellersSearchService = new Mock<ISellersSearchService>();

            var result = await new SearchSellerCommissionsQueryHandler(searchService.Object, sellersSearchService.Object).Handle(query, CancellationToken.None);

            Assert.Same(expected, result);
            sellersSearchService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handler_Keyword_SearchesAssignmentsOfMatchingVendors()
        {
            var query = new SearchSellerCommissionsQuery { CommissionFeeId = StaticFeeId, Keyword = "Acme", Skip = 20, Take = 10 };
            SearchSellerCommissionsQuery actual = null;
            var expected = new SearchSellerCommissionsResult { TotalCount = 2 };
            var searchService = new Mock<ISellerCommissionSearchService>();
            searchService
                .Setup(x => x.SearchAsync(It.IsAny<SearchSellerCommissionsQuery>(), It.IsAny<bool>()))
                .Callback<SearchSellerCommissionsQuery, bool>((x, _) => actual = x)
                .ReturnsAsync(expected);

            var result = await new SearchSellerCommissionsQueryHandler(searchService.Object, SellersSearch("Acme", "seller1", "seller3").Object)
                .Handle(query, CancellationToken.None);

            Assert.Same(expected, result);
            Assert.NotSame(query, actual);
            Assert.Null(actual.Keyword);
            Assert.Equal(["seller1", "seller3"], actual.SellerIds);
            Assert.Equal(StaticFeeId, actual.CommissionFeeId);
            Assert.Equal(20, actual.Skip);
            Assert.Equal(10, actual.Take);
            Assert.Equal("Acme", query.Keyword);
        }

        [Fact]
        public async Task Handler_KeywordAndSellerIds_SearchesIntersection()
        {
            var query = new SearchSellerCommissionsQuery { CommissionFeeId = StaticFeeId, Keyword = "Acme", SellerIds = ["seller3", "seller9"] };
            SearchSellerCommissionsQuery actual = null;
            var searchService = new Mock<ISellerCommissionSearchService>();
            searchService
                .Setup(x => x.SearchAsync(It.IsAny<SearchSellerCommissionsQuery>(), It.IsAny<bool>()))
                .Callback<SearchSellerCommissionsQuery, bool>((x, _) => actual = x)
                .ReturnsAsync(new SearchSellerCommissionsResult());

            await new SearchSellerCommissionsQueryHandler(searchService.Object, SellersSearch("Acme", "seller1", "seller3").Object)
                .Handle(query, CancellationToken.None);

            Assert.Equal(["seller3"], actual.SellerIds);
        }

        [Fact]
        public async Task Handler_KeywordWithoutMatchingVendors_ReturnsEmptyWithoutSearching()
        {
            var query = new SearchSellerCommissionsQuery { CommissionFeeId = StaticFeeId, Keyword = "nothing" };
            var searchService = new Mock<ISellerCommissionSearchService>();

            var result = await new SearchSellerCommissionsQueryHandler(searchService.Object, SellersSearch("nothing").Object)
                .Handle(query, CancellationToken.None);

            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Results);
            searchService.VerifyNoOtherCalls();
        }

        private static Mock<ISellersSearchService> SellersSearch(string keyword, params string[] sellerIds)
        {
            var sellers = sellerIds.Select(id => new Seller { Id = id }).ToList();
            var sellersSearchService = new Mock<ISellersSearchService>();
            sellersSearchService
                .Setup(x => x.SearchAsync(It.Is<SearchSellersQuery>(q => q.Keyword == keyword), It.IsAny<bool>()))
                .ReturnsAsync((SearchSellersQuery q, bool _) => new SearchSellersResult
                {
                    TotalCount = sellers.Count,
                    Results = sellers.Skip(q.Skip).Take(q.Take).ToList(),
                });

            return sellersSearchService;
        }

        private static List<SellerCommissionEntity> Search(SearchSellerCommissionsQuery criteria)
        {
            var repository = new Mock<ICommissionFeeRepository>();
            repository.SetupGet(x => x.SellerCommissions).Returns(SellerCommissions.AsQueryable());

            var service = new TestSellerCommissionSearchService(() => repository.Object);

            return service.BuildQueryPublic(repository.Object, criteria).ToList();
        }

        private static SellerCommissionEntity NewSellerCommission(string id, string feeId, string sellerId)
        {
            return new SellerCommissionEntity
            {
                Id = id,
                CommissionFeeId = feeId,
                SellerId = sellerId,
            };
        }

        private sealed class TestSellerCommissionSearchService : SellerCommissionSearchService
        {
            public TestSellerCommissionSearchService(System.Func<ICommissionFeeRepository> repositoryFactory)
                : base(repositoryFactory, Mock.Of<IPlatformMemoryCache>(), Mock.Of<ISellerCommissionCrudService>(), Options.Create(new CrudOptions()))
            {
            }

            public IQueryable<SellerCommissionEntity> BuildQueryPublic(IRepository repository, SearchSellerCommissionsQuery criteria)
            {
                return BuildQuery(repository, criteria);
            }

            public IList<SortInfo> BuildSortExpressionPublic(SearchSellerCommissionsQuery criteria)
            {
                return BuildSortExpression(criteria);
            }
        }
    }
}
