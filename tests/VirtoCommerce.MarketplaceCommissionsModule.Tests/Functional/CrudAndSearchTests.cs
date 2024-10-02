using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Moq;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Services;
using VirtoCommerce.Platform.Core.Caching;
using Xunit;

namespace VirtoCommerce.MarketplaceCommissionsModule.Tests.Functional
{
    public class CrudAndSearchTests
    {
        private const string _commissionFeeId = "CommissionFeeTestId";
        private const string _commissionFeeType = "Static";
        private const bool _commissionFeeIsDefault = true;
        private static CommissionFee _commissionFee = new()
        {
            Id = "CommissionFeeTestId",
            Name = "My test commission",
            Type = CommissionFeeType.Static,
            CalculationType = FeeCalculationType.Fixed,
            Fee = 1,
            Priority = 0,
            IsDefault = true,
        };

        private readonly Mock<IPlatformMemoryCache> _platformMemoryCacheMock;
        private readonly Mock<ICacheEntry> _cacheEntryMock;
        private ICommissionFeeService _commissionFeeService;
        private ICommissionFeeSearchService _commissionFeeSearchService;

        public CrudAndSearchTests()
        {
            _cacheEntryMock = new Mock<ICacheEntry>();
            _cacheEntryMock.SetupGet(c => c.ExpirationTokens).Returns(new List<IChangeToken>());
            _platformMemoryCacheMock = new Mock<IPlatformMemoryCache>();
            _platformMemoryCacheMock.Setup(x => x.GetDefaultCacheEntryOptions()).Returns(() => new MemoryCacheEntryOptions());
            var cacheKeyCrud = CacheKey.With(typeof(CommissionFeeService), "GetAsync", string.Join("-", _commissionFeeId), null);
            _platformMemoryCacheMock.Setup(pmc => pmc.CreateEntry(cacheKeyCrud)).Returns(_cacheEntryMock.Object);
        }

        [Fact]
        public async Task CanDoCrudAndSearch()
        {
            MockServices(_commissionFeeId, _commissionFeeType, _commissionFeeIsDefault);

            // Create
            var item = new CommissionFee
            {
                Id = _commissionFeeId,
                Description = "Test description",
                Type = CommissionFeeType.Static,
                IsDefault = true,
                CreatedDate = DateTime.Now,
                CreatedBy = "Initial data seed"
            };

            await _commissionFeeService.SaveChangesAsync(new[] { item });

            var result = await _commissionFeeService.GetAsync(new[] { _commissionFeeId });
            Assert.Single(result);
            item = result.First();
            Assert.Equal(_commissionFeeId, item.Id);

            // Update
            var updatedDescription = "Updated description";
            Assert.NotEqual(updatedDescription, item.Description);

            item.Description = updatedDescription;
            await _commissionFeeService.SaveChangesAsync(new[] { item });

            result = await _commissionFeeService.GetAsync([_commissionFeeId]);
            Assert.Single(result);

            item = result.First();
            Assert.Equal(updatedDescription, item.Description);

            var criteria = new SearchCommissionFeesQuery { Type = (CommissionFeeType)Enum.Parse(typeof(CommissionFeeType), _commissionFeeType), IsDefault = _commissionFeeIsDefault };
            var cacheKeySearch = CacheKey.With(typeof(CommissionFeeSearchService), "SearchAsync", criteria.GetCacheKey());
            _platformMemoryCacheMock.Setup(pmc => pmc.CreateEntry(cacheKeySearch)).Returns(_cacheEntryMock.Object);

            var searchResult = await _commissionFeeSearchService.SearchAsync(criteria);
            Assert.NotNull(searchResult);
            Assert.Equal(1, searchResult.TotalCount);
            Assert.Single(searchResult.Results);

            // Delete
            await _commissionFeeService.DeleteAsync(new[] { _commissionFeeId });

            var getByIdsResult = await _commissionFeeService.GetAsync(new[] { _commissionFeeId });
            Assert.NotNull(getByIdsResult);
        }

        private void MockServices(string commissionFeeId, string commissionFeeType, bool commissionFeeIsDefault)
        {
            var commissionFeeService = new Mock<ICommissionFeeService>();

            CommissionFee[] commissionFees = [_commissionFee];

            commissionFeeService
                .Setup(x => x.GetAsync(new[] { _commissionFeeId }, It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult((IList<CommissionFee>)commissionFees.Where(x => x.Id == commissionFeeId).ToList()));

            commissionFeeService
                .Setup(x => x.SaveChangesAsync(It.IsAny<CommissionFee[]>()))
                .Returns(Task.CompletedTask);
            commissionFeeService
                .Setup(x => x.DeleteAsync(It.IsAny<List<string>>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            _commissionFeeService = commissionFeeService.Object;

            var commissionFeeSearchService = new Mock<ICommissionFeeSearchService>();

            commissionFeeSearchService
                .Setup(x => x.SearchAsync(new SearchCommissionFeesQuery { Type = (CommissionFeeType)Enum.Parse(typeof(CommissionFeeType), commissionFeeType), IsDefault = commissionFeeIsDefault }, It.IsAny<bool>()))
                .Returns(Task.FromResult(new SearchCommissionFeesResult
                {
                    Results = commissionFees.Where(x => x.Type == (CommissionFeeType)Enum.Parse(typeof(CommissionFeeType), commissionFeeType) && x.IsDefault == commissionFeeIsDefault).ToList(),
                    TotalCount = commissionFees.Count(x => x.Type == (CommissionFeeType)Enum.Parse(typeof(CommissionFeeType), commissionFeeType) && x.IsDefault == commissionFeeIsDefault)
                }));

            _commissionFeeSearchService = commissionFeeSearchService.Object;
        }
    }
}
