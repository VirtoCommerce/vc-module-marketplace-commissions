using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Repositories
{
    public class CommissionFeeRepository : DbContextRepositoryBase<CommissionFeeDbContext>, ICommissionFeeRepository
    {
        public CommissionFeeRepository(CommissionFeeDbContext dbContext)
            : base(dbContext)
        {
        }

        public IQueryable<CommissionFeeEntity> CommissionFees => DbContext.Set<CommissionFeeEntity>();
        public IQueryable<SellerCommissionEntity> SellerCommissions => DbContext.Set<SellerCommissionEntity>();

        public virtual async Task<CommissionFeeEntity[]> GetCommissionFeesByIds(string[] ids, string responseGroup = null)
        {
            var result = Array.Empty<CommissionFeeEntity>();

            if (!ids.IsNullOrEmpty())
            {
                result = await CommissionFees.Where(x => ids.Contains(x.Id)).ToArrayAsync();
            }
            return result;
        }

        public virtual async Task<SellerCommissionEntity[]> GetSellerCommissionsByIds(string[] ids, string responseGroup = null)
        {
            var result = Array.Empty<SellerCommissionEntity>();

            if (!ids.IsNullOrEmpty())
            {
                result = await SellerCommissions.Where(x => ids.Contains(x.Id)).ToArrayAsync();
            }
            return result;
        }
    }
}
