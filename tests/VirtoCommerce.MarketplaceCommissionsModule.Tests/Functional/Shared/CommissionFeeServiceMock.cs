using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;

namespace VirtoCommerce.MarketplaceCommissionsModule.Tests.Functional
{
    public class CommissionFeeServiceMock : ICommissionFeeService
    {
        public List<CommissionFee> CommissionFees = new();

        public Task<CommissionFee> GetDefaultCommissionFee()
        {
            return Task.FromResult(CommissionFees.FirstOrDefault(x => x.IsDefault == true));
        }

        public Task<IList<CommissionFee>> GetAsync(IList<string> ids, string responseGroup = null, bool clone = true)
        {
            return Task.FromResult<IList<CommissionFee>>(CommissionFees.Where(x => ids.Contains(x.Id)).ToList());
        }

        public Task SaveChangesAsync(IList<CommissionFee> models)
        {
            foreach (var item in models)
            {
                if (item.Id == null)
                    item.Id = Guid.NewGuid().ToString();
                var commissionFee = CommissionFees.FirstOrDefault(x => x.Id == item.Id);
                if (commissionFee != null)
                    commissionFee = item;
                else
                    CommissionFees.Add(item);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(IList<string> ids, bool softDelete = false)
        {
            CommissionFees.RemoveAll(x => ids.Contains(x.Id));
            return Task.CompletedTask;
        }

        public Task<CommissionFee> GetCommissionBySellerId(string sellerId)
        {
            throw new NotImplementedException();
        }
    }
}
