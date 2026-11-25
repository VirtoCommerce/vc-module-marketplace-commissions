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
using Xunit;

namespace VirtoCommerce.MarketplaceCommissionsModule.Tests.Unit
{
    public class DeleteFeeCommandHandlerTests
    {
        [Fact]
        public async Task Handle_StaticFeeWithoutVendors_Deletes()
        {
            var context = new TestContext(new CommissionFee { Id = "fee", Type = CommissionFeeType.Static });

            await context.Handle("fee");

            Assert.Equal(["fee"], context.DeletedIds);
            var query = Assert.Single(context.Queries);
            Assert.Equal("fee", query.CommissionFeeId);
            Assert.Equal(0, query.Take);
        }

        [Fact]
        public async Task Handle_StaticFeeWithVendors_ThrowsAndKeepsFee()
        {
            var context = new TestContext(new CommissionFee { Id = "fee", Type = CommissionFeeType.Static }, assignedSellers: 3);

            var exception = await Assert.ThrowsAsync<OperationCanceledException>(() => context.Handle("fee"));

            Assert.Contains("3 vendor", exception.Message);
            Assert.Empty(context.DeletedIds);
        }

        [Fact]
        public async Task Handle_DefaultFee_ThrowsAndKeepsFee()
        {
            var context = new TestContext(new CommissionFee { Id = "fee", Type = CommissionFeeType.Static, IsDefault = true });

            await Assert.ThrowsAsync<OperationCanceledException>(() => context.Handle("fee"));

            Assert.Empty(context.DeletedIds);
        }

        [Fact]
        public async Task Handle_DynamicFee_DeletesWithoutCheckingVendors()
        {
            var context = new TestContext(new CommissionFee { Id = "fee", Type = CommissionFeeType.Dynamic }, assignedSellers: 3);

            await context.Handle("fee");

            Assert.Equal(["fee"], context.DeletedIds);
            Assert.Empty(context.Queries);
        }

        [Fact]
        public async Task Handle_UnknownFee_DoesNothing()
        {
            var context = new TestContext(fee: null);

            await context.Handle("unknown");

            Assert.Empty(context.DeletedIds);
        }

        private sealed class TestContext
        {
            private readonly Mock<ICommissionFeeService> _feeService = new();
            private readonly Mock<ISellerCommissionSearchService> _searchService = new();

            public List<string> DeletedIds { get; } = [];
            public List<SearchSellerCommissionsQuery> Queries { get; } = [];

            public TestContext(CommissionFee fee, int assignedSellers = 0)
            {
                _feeService
                    .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync((IList<string> ids, string _, bool _) => fee != null && ids.Contains(fee.Id) ? [fee] : []);
                _feeService
                    .Setup(x => x.DeleteAsync(It.IsAny<IList<string>>(), It.IsAny<bool>()))
                    .Callback<IList<string>, bool>((ids, _) => DeletedIds.AddRange(ids))
                    .Returns(Task.CompletedTask);

                _searchService
                    .Setup(x => x.SearchAsync(It.IsAny<SearchSellerCommissionsQuery>(), It.IsAny<bool>()))
                    .ReturnsAsync((SearchSellerCommissionsQuery q, bool _) =>
                    {
                        Queries.Add(q);
                        return new SearchSellerCommissionsResult { TotalCount = assignedSellers, Results = [] };
                    });
            }

            public Task Handle(string id)
            {
                return new DeleteFeeCommandHandler(_feeService.Object, _searchService.Object).Handle(new DeleteFeeCommand { Id = id }, CancellationToken.None);
            }
        }
    }
}
