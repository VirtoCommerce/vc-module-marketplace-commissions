using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Commands
{
    public class BulkUpdateSellerCommissionsCommandHandler : ICommandHandler<BulkUpdateSellerCommissionsCommand, BulkUpdateSellerCommissionsResult>
    {
        protected virtual int SearchPageSize => 100;
        protected virtual int SaveBatchSize => 50;

        private readonly IMediator _mediator;
        private readonly ICommissionFeeService _commissionFeeService;
        private readonly ISellerCommissionCrudService _sellerCommissionCrudService;

        public BulkUpdateSellerCommissionsCommandHandler(IMediator mediator, ICommissionFeeService commissionFeeService, ISellerCommissionCrudService sellerCommissionCrudService)
        {
            _mediator = mediator;
            _commissionFeeService = commissionFeeService;
            _sellerCommissionCrudService = sellerCommissionCrudService;
        }

        public virtual async Task<BulkUpdateSellerCommissionsResult> Handle(BulkUpdateSellerCommissionsCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request);

            var result = ExType<BulkUpdateSellerCommissionsResult>.New();

            var sellerCommissions = await FindSellerCommissionsAsync(request, cancellationToken);
            if (!request.ExcludedSellerIds.IsNullOrEmpty())
            {
                var excluded = request.ExcludedSellerIds.ToHashSet();
                sellerCommissions = sellerCommissions.Where(x => !excluded.Contains(x.SellerId)).ToList();
            }

            foreach (var batch in sellerCommissions.Chunk(SaveBatchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var sellerCommission in batch)
                {
                    sellerCommission.CommissionFeeId = request.TargetCommissionFeeId;
                }

                await _sellerCommissionCrudService.SaveChangesAsync(batch);
                result.UpdatedCount += batch.Length;
            }

            return result;
        }

        protected virtual async Task ValidateAsync(BulkUpdateSellerCommissionsCommand request)
        {
            if (string.IsNullOrEmpty(request.CommissionFeeId) || string.IsNullOrEmpty(request.TargetCommissionFeeId))
            {
                throw new OperationCanceledException("Both the current and the target commission fees are required");
            }

            if (request.CommissionFeeId == request.TargetCommissionFeeId)
            {
                throw new OperationCanceledException("The target commission fee is the current one");
            }

            var target = (await _commissionFeeService.GetAsync([request.TargetCommissionFeeId])).FirstOrDefault();
            if (target == null)
            {
                throw new OperationCanceledException($"CommissionFee with {request.TargetCommissionFeeId} is not found");
            }

            if (target.Type != CommissionFeeType.Static)
            {
                throw new OperationCanceledException("Vendors can be assigned to a static commission fee only");
            }
        }

        /// <summary>
        /// The assignments to the current commission fee: the selected vendors, or all vendors matching the keyword.
        /// All pages are read before any change, since the changes take the assignments out of the search.
        /// </summary>
        protected virtual async Task<IList<SellerCommission>> FindSellerCommissionsAsync(BulkUpdateSellerCommissionsCommand request, CancellationToken cancellationToken)
        {
            var result = new List<SellerCommission>();

            if (!request.AllSellers && request.SellerIds.IsNullOrEmpty())
            {
                return result;
            }

            var query = ExType<SearchSellerCommissionsQuery>.New();
            query.CommissionFeeId = request.CommissionFeeId;
            query.SellerIds = request.AllSellers ? null : request.SellerIds;
            query.Keyword = request.AllSellers ? request.Keyword : null;
            query.Take = SearchPageSize;

            int totalCount;
            do
            {
                var page = await _mediator.Send(query, cancellationToken);
                result.AddRange(page.Results);
                totalCount = page.TotalCount;
                query.Skip += SearchPageSize;
            }
            while (query.Skip < totalCount);

            return result;
        }
    }
}
