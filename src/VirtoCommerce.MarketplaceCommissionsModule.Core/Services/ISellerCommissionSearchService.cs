using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
public interface ISellerCommissionSearchService : ISearchService<SearchSellerCommissionsQuery, SearchSellerCommissionsResult, SellerCommission>
{
}
