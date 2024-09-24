using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;

namespace VirtoCommerce.MarketplaceCommissionsModule.Tests.Functional
{
    public class CommissionFeeSearchServiceStub : ICommissionFeeSearchService
    {
        public Task<SearchCommissionFeesResult> SearchAsync(SearchCommissionFeesQuery criteria, bool clone = true)
        {
            return Task.FromResult(new SearchCommissionFeesResult());
        }
    }
}
