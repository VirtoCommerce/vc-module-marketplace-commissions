using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Services
{
    public interface ICommissionFeeEvaluator
    {
        Task<EntryFee[]> EvaluateFeeAsync(CommissionFeeEvaluationContext context);
    }
}
