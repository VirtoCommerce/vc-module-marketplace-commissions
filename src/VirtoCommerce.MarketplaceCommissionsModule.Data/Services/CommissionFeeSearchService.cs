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

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Services
{
    public class CommissionFeeSearchService : SearchService<SearchCommissionFeesQuery, SearchCommissionFeesResult, CommissionFee, CommissionFeeEntity>,
        ICommissionFeeSearchService
    {
        public CommissionFeeSearchService(
            Func<ICommissionFeeRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            ICommissionFeeService crudService,
            IOptions<CrudOptions> crudOptions)
            : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
        {
        }

        protected override IQueryable<CommissionFeeEntity> BuildQuery(IRepository repository, SearchCommissionFeesQuery criteria)
        {
            var query = ((ICommissionFeeRepository)repository).CommissionFees;

            if (criteria.Type != null)
            {
                query = query.Where(x => x.Type == criteria.Type);
            }

            if (criteria.IsDefault != null)
            {
                query = query.Where(x => x.IsDefault == criteria.IsDefault);
            }

            if (criteria.IsActive != null)
            {
                query = query.Where(x => x.IsActive == criteria.IsActive);
            }

            return query;
        }

        protected override IList<SortInfo> BuildSortExpression(SearchCommissionFeesQuery criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo
                    {
                        SortColumn = ReflectionUtility.GetPropertyName<CommissionFeeEntity>(x => x.CreatedDate),
                        SortDirection = SortDirection.Descending
                    }
                };
            }

            return sortInfos;
        }
    }
}
