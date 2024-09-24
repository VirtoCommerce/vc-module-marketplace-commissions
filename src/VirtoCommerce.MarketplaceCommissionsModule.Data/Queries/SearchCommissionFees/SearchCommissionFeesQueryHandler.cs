using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Queries
{
    public class SearchCommissionFeesQueryHandler : IQueryHandler<SearchCommissionFeesQuery, SearchCommissionFeesResult>
    {
        private readonly ICommissionFeeSearchService _searchService;
        public SearchCommissionFeesQueryHandler(ICommissionFeeSearchService searchService)
        {
            _searchService = searchService;
        }
        public virtual async Task<SearchCommissionFeesResult> Handle(SearchCommissionFeesQuery request, CancellationToken cancellationToken)
        {
            return await _searchService.SearchAsync(request);
        }
    }
}
