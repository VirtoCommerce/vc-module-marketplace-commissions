using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Models;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.MarketplaceCommissionsModule.Tests.Functional
{
    public class CommissionFeeRepositoryMock : ICommissionFeeRepository
    {
        public IQueryable<CommissionFeeEntity> CommissionFees => throw new NotImplementedException();
        public List<CommissionFeeEntity> CommissionFeesEntity = new();

        public IQueryable<SellerCommissionEntity> SellerCommissions => throw new NotImplementedException();
        public List<SellerCommissionEntity> SellerCommissionsEntity = new();

        public IUnitOfWork UnitOfWork => new Mock<IUnitOfWork>().Object;

        public void Add<T>(T item) where T : class
        {
            if (item.GetType() == typeof(CommissionFeeEntity))
            {
                var feeEntity = item as CommissionFeeEntity;
                feeEntity.Id = nameof(CommissionFeeEntity) + feeEntity.Name;
                CommissionFeesEntity.Add(feeEntity);
            }
            else if (item.GetType() == typeof(SellerCommissionEntity))
            {
                var sellerCommissionEntity = item as SellerCommissionEntity;
                sellerCommissionEntity.Id = nameof(SellerCommissionEntity) + sellerCommissionEntity.Id;
                SellerCommissionsEntity.Add(sellerCommissionEntity);
            }
        }

        public void Attach<T>(T item) where T : class
        {
            throw new NotImplementedException();
        }

        public void Remove<T>(T item) where T : class
        {
            if (item.GetType() == typeof(CommissionFeeEntity))
            {
                var feeEntity = item as CommissionFeeEntity;
                CommissionFeesEntity.Remove(feeEntity);
            }
            else if (item.GetType() == typeof(SellerCommissionEntity))
            {
                var sellerCommissionEntity = item as SellerCommissionEntity;
                SellerCommissionsEntity.Remove(sellerCommissionEntity);
            }
        }

        public void Update<T>(T item) where T : class
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public Task<CommissionFeeEntity[]> GetCommissionFeesByIds(string[] ids, string responseGroup = null)
        {
            var result = Array.Empty<CommissionFeeEntity>();
            if (!ids.IsNullOrEmpty())
            {
                result = CommissionFeesEntity.Where(x => ids.Contains(x.Id)).ToArray();
            }
            return Task.FromResult(result);
        }

        public Task<SellerCommissionEntity[]> GetSellerCommissionsByIds(string[] ids, string responseGroup = null)
        {
            throw new NotImplementedException();
        }
    }
}
