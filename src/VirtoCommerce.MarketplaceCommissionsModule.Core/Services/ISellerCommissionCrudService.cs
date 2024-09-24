using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Services
{
    public interface ISellerCommissionCrudService : ICrudService<SellerCommission>
    {
        Task<SellerCommission> GetSellerCommission(string sellerId);
        Task<IList<SellerCommission>> GetSellersCommissions(IList<string> sellerIds);
    }
}
