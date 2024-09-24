using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Models;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Repositories;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;
using VirtoCommerce.MarketplaceVendorModule.Data.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Services
{
    public class SellerCommissionCrudService : AggregateRootCrudServiceBase<SellerCommission, SellerCommissionEntity>, ISellerCommissionCrudService
    {
        private readonly Func<ICommissionFeeRepository> _repositoryFactory;

        public SellerCommissionCrudService(
            IMediator mediator,
            Func<ICommissionFeeRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher)
            : base(mediator, repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _repositoryFactory = repositoryFactory;
        }

        protected override async Task<IList<SellerCommissionEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
        {
            return await ((ICommissionFeeRepository)repository).GetSellerCommissionsByIds(ids.ToArray(), responseGroup);
        }

        public virtual async Task<SellerCommission> GetSellerCommission(string sellerId)
        {
            SellerCommission result = null;

            using var repository = _repositoryFactory();
            var entity = await repository.SellerCommissions.FirstOrDefaultAsync(x => x.SellerId == sellerId);
            if (entity != null)
            {
                result = entity.ToModel(ExType<SellerCommission>.New());
            }

            return result;
        }

        public virtual async Task<IList<SellerCommission>> GetSellersCommissions(IList<string> sellerIds)
        {
            IList<SellerCommission> result = new List<SellerCommission>();

            using var repository = _repositoryFactory();
            var sellerCommissions = await repository.SellerCommissions.Where(x => sellerIds.Contains(x.SellerId)).ToListAsync();
            if (sellerCommissions != null && sellerCommissions.Any())
            {
                result = sellerCommissions.Select(x => x.ToModel(ExType<SellerCommission>.New())).ToList();
            }

            return result;
        }

    }
}
