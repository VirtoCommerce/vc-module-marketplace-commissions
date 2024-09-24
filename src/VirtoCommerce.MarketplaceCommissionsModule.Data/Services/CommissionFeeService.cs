using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Models;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Repositories;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;
using VirtoCommerce.MarketplaceVendorModule.Data.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using ICommissionFeeService = VirtoCommerce.MarketplaceCommissionsModule.Core.Services.ICommissionFeeService;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Services
{
    public class CommissionFeeService : AggregateRootCrudServiceBase<CommissionFee, CommissionFeeEntity>, ICommissionFeeService
    {
        private readonly Func<ICommissionFeeRepository> _repositoryFactory;

        public CommissionFeeService(
            IMediator mediator,
            Func<ICommissionFeeRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher)
            : base(mediator, repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _repositoryFactory = repositoryFactory;
        }

        protected override async Task<IList<CommissionFeeEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
        {
            return await ((ICommissionFeeRepository)repository).GetCommissionFeesByIds(ids.ToArray(), responseGroup);
        }

        protected override CommissionFee ProcessModel(string responseGroup, CommissionFeeEntity entity, CommissionFee model)
        {
            return model.Type == CommissionFeeType.Dynamic ? entity.ToModel(AbstractTypeFactory<DynamicCommissionFee>.TryCreateInstance()) : model;
        }

        public virtual async Task<CommissionFee> GetDefaultCommissionFee()
        {
            CommissionFee result = null;

            using var repository = _repositoryFactory();
            var entity = await repository.CommissionFees.FirstOrDefaultAsync(x => x.IsDefault == true);
            if (entity != null)
            {
                result = entity.ToModel(ExType<CommissionFee>.New());
            }

            return result;
        }

        public virtual async Task<CommissionFee> GetCommissionBySellerId(string sellerId)
        {
            CommissionFee result = null;

            using var repository = _repositoryFactory();
            var entity = await repository.SellerCommissions.Where(x => x.SellerId == sellerId).Include(x => x.CommissionFee).FirstOrDefaultAsync();
            if (entity != null)
            {
                var sellerCommission = entity.CommissionFee;
                result = sellerCommission.ToModel(ExType<CommissionFee>.New());
            }

            return result;
        }

    }
}
