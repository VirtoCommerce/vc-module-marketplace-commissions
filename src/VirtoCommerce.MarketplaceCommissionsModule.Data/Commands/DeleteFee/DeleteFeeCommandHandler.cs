using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Commands
{
    public class DeleteFeeCommandHandler : ICommandHandler<DeleteFeeCommand>
    {
        private readonly ICommissionFeeService _commissionFeeService;

        public DeleteFeeCommandHandler(ICommissionFeeService commissionFeeService)
        {
            _commissionFeeService = commissionFeeService;
        }

        public virtual async Task<Unit> Handle(DeleteFeeCommand request, CancellationToken cancellationToken)
        {
            var commissionFee = await _commissionFeeService.GetByIdAsync(request.Id);
            if (commissionFee.IsDefault)
            {
                throw new OperationCanceledException("Unable to delete default fee");
            }
            await _commissionFeeService.DeleteAsync(new[] { request.Id });
            return Unit.Value;
        }
    }
}
