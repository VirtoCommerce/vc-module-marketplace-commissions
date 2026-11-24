using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;
using VirtoCommerce.MarketplaceVendorModule.Data.Queries;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Queries
{
    public class SearchSellerCommissionsQueryHandler : IQueryHandler<SearchSellerCommissionsQuery, SearchSellerCommissionsResult>
    {
        private readonly ISellerCommissionSearchService _searchService;
        private readonly ISellersSearchService _sellersSearchService;

        public SearchSellerCommissionsQueryHandler(ISellerCommissionSearchService searchService, ISellersSearchService sellersSearchService)
        {
            _searchService = searchService;
            _sellersSearchService = sellersSearchService;
        }

        public virtual async Task<SearchSellerCommissionsResult> Handle(SearchSellerCommissionsQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Keyword))
            {
                return await _searchService.SearchAsync(request);
            }

            // The keyword matches vendor names, which belong to MarketplaceVendor: find the vendors there,
            // then search the assignments of those vendors only
            var sellersQuery = ExType<SearchSellersQuery>.New();
            sellersQuery.Keyword = request.Keyword;
            var sellers = await _sellersSearchService.SearchAllNoCloneAsync(sellersQuery);
            var sellerIds = sellers.Select(x => x.Id).ToList();
            if (!request.SellerIds.IsNullOrEmpty())
            {
                sellerIds = sellerIds.Intersect(request.SellerIds).ToList();
            }

            if (sellerIds.Count == 0)
            {
                return ExType<SearchSellerCommissionsResult>.New();
            }

            var criteria = request.CloneTyped();
            criteria.Keyword = null;
            criteria.SellerIds = sellerIds;

            return await _searchService.SearchAsync(criteria);
        }
    }
}
