using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Commands
{
    public class UpdateFeeCommandHandler : ICommandHandler<UpdateFeeCommand, CommissionFee>
    {
        private readonly ICommissionFeeService _commissionFeeService;

        public UpdateFeeCommandHandler(ICommissionFeeService commissionFeeService)
        {
            _commissionFeeService = commissionFeeService;
        }

        public virtual async Task<CommissionFee> Handle(UpdateFeeCommand request, CancellationToken cancellationToken)
        {
            var commissionFee = await _commissionFeeService.GetByIdAsync(request.FeeDetails.Id);
            if (commissionFee == null)
            {
                throw new OperationCanceledException($"CommissionFee with {request.FeeDetails.Id} is not found");
            }

            commissionFee.Update(request.FeeDetails);

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

            return await _commissionFeeService.GetByIdAsync(commissionFee.Id);
        }
    }
}
