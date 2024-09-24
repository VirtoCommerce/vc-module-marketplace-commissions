using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Services
{
    public interface ICommissionFeeService : ICrudService<CommissionFee>
    {
        Task<CommissionFee> GetDefaultCommissionFee();
        Task<CommissionFee> GetCommissionBySellerId(string sellerId);
    }
}
