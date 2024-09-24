using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Commands
{
    public class CreateFeeCommandHandler : ICommandHandler<CreateFeeCommand, CommissionFee>
    {
        private readonly ICommissionFeeService _commissionFeeService;

        public CreateFeeCommandHandler(ICommissionFeeService commissionFeeService)
        {
            _commissionFeeService = commissionFeeService;
        }

        public virtual async Task<CommissionFee> Handle(CreateFeeCommand request, CancellationToken cancellationToken)
        {
            var commissionFee = request.FeeDetails.Type == CommissionFeeType.Dynamic
                ? DynamicCommissionFee.CreateNew(request.FeeDetails)
                : CommissionFee.CreateNew(request.FeeDetails);

            var commissionFees = new List<CommissionFee> { commissionFee };

            if (request.FeeDetails.IsDefault)
            {
                var previousDefaultFee = await _commissionFeeService.GetDefaultCommissionFee();
                commissionFee.SetAsDefault(previousDefaultFee);
                if (previousDefaultFee != null && previousDefaultFee.Id != commissionFee.Id)
                {
                    commissionFees.Add(previousDefaultFee);
                }
            }

            await _commissionFeeService.SaveChangesAsync(commissionFees);

            return commissionFee;
        }
    }
}
