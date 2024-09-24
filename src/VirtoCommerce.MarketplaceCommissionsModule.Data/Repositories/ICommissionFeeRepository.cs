using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Repositories
{
    public interface ICommissionFeeRepository : IRepository
    {
        IQueryable<CommissionFeeEntity> CommissionFees { get; }
        IQueryable<SellerCommissionEntity> SellerCommissions { get; }

        Task<CommissionFeeEntity[]> GetCommissionFeesByIds(string[] ids, string responseGroup = null);
        Task<SellerCommissionEntity[]> GetSellerCommissionsByIds(string[] ids, string responseGroup = null);
    }
}
