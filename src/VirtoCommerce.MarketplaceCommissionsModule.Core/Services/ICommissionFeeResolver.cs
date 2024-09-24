using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Services
{
    public interface ICommissionFeeResolver
    {
        Task<IDictionary<string, CommissionFee>> ResolveBySellerIds(string[] sellerIds);
    }
}
