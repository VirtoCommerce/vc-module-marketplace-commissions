using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Models;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Services;
public class SellerCommissionSearchService : SearchService<SearchSellerCommissionsQuery, SearchSellerCommissionsResult, SellerCommission, SellerCommissionEntity>,
    ISellerCommissionSearchService
{
    public SellerCommissionSearchService(
    Func<ICommissionFeeRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    ISellerCommissionCrudService crudService,
    IOptions<CrudOptions> crudOptions)
    : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
    }

    protected override IQueryable<SellerCommissionEntity> BuildQuery(IRepository repository, SearchSellerCommissionsQuery criteria)
    {
        var query = ((ICommissionFeeRepository)repository).SellerCommissions;

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(SearchSellerCommissionsQuery criteria)
    {
        var sortInfos = criteria.SortInfos;
        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
                [
                    new SortInfo
                    {
                        SortColumn = ReflectionUtility.GetPropertyName<SellerCommissionEntity>(x => x.CreatedDate),
                        SortDirection = SortDirection.Descending
                    }
                ];
        }

        return sortInfos;
    }


}
