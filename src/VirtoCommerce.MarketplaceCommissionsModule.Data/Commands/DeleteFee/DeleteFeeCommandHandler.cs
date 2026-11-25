using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Commands
{
    public class DeleteFeeCommandHandler : ICommandHandler<DeleteFeeCommand>
    {
        private readonly ICommissionFeeService _commissionFeeService;
        private readonly ISellerCommissionSearchService _sellerCommissionSearchService;

        public DeleteFeeCommandHandler(ICommissionFeeService commissionFeeService, ISellerCommissionSearchService sellerCommissionSearchService)
        {
            _commissionFeeService = commissionFeeService;
            _sellerCommissionSearchService = sellerCommissionSearchService;
        }

        public virtual async Task Handle(DeleteFeeCommand request, CancellationToken cancellationToken)
        {
            var commissionFee = await _commissionFeeService.GetByIdAsync(request.Id);
            if (commissionFee == null)
            {
                return;
            }

            if (commissionFee.IsDefault)
            {
                throw new OperationCanceledException("Unable to delete default fee");
            }

            // vendor assignments are deleted in cascade with the fee, which would leave the vendors without a static commission
            if (commissionFee.Type == CommissionFeeType.Static)
            {
                var assignedCount = await GetAssignedSellersCountAsync(commissionFee.Id);
                if (assignedCount > 0)
                {
                    throw new OperationCanceledException($"Unable to delete the commission fee: {assignedCount} vendor(s) are assigned to it. Assign them to another static commission fee first");
                }
            }

            await _commissionFeeService.DeleteAsync([request.Id]);
        }

        protected virtual async Task<int> GetAssignedSellersCountAsync(string commissionFeeId)
        {
            var criteria = ExType<SearchSellerCommissionsQuery>.New();
            criteria.CommissionFeeId = commissionFeeId;
            criteria.Take = 0;

            var result = await _sellerCommissionSearchService.SearchAsync(criteria, clone: false);

            return result.TotalCount;
        }
    }
}
