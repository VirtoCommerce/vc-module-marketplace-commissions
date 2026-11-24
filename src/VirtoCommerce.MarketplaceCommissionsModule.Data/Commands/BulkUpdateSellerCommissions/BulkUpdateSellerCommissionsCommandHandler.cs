using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;
using VirtoCommerce.MarketplaceVendorModule.Data.Queries;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Commands
{
    public class BulkUpdateSellerCommissionsCommandHandler : ICommandHandler<BulkUpdateSellerCommissionsCommand, BulkUpdateSellerCommissionsResult>
    {
        protected virtual int SearchPageSize => 100;
        protected virtual int SaveBatchSize => 50;

        private readonly ICommissionFeeService _commissionFeeService;
        private readonly ISellerCommissionSearchService _sellerCommissionSearchService;
        private readonly ISellerCommissionCrudService _sellerCommissionCrudService;
        private readonly ISellersSearchService _sellersSearchService;

        public BulkUpdateSellerCommissionsCommandHandler(
            ICommissionFeeService commissionFeeService,
            ISellerCommissionSearchService sellerCommissionSearchService,
            ISellerCommissionCrudService sellerCommissionCrudService,
            ISellersSearchService sellersSearchService)
        {
            _commissionFeeService = commissionFeeService;
            _sellerCommissionSearchService = sellerCommissionSearchService;
            _sellerCommissionCrudService = sellerCommissionCrudService;
            _sellersSearchService = sellersSearchService;
        }

        public virtual async Task<BulkUpdateSellerCommissionsResult> Handle(BulkUpdateSellerCommissionsCommand request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request);

            var result = ExType<BulkUpdateSellerCommissionsResult>.New();

            var sellerCommissions = await FindSellerCommissionsAsync(request);
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
        protected virtual async Task<IList<SellerCommission>> FindSellerCommissionsAsync(BulkUpdateSellerCommissionsCommand request)
        {
            if (!request.AllSellers && request.SellerIds.IsNullOrEmpty())
            {
                return [];
            }

            var sellerIds = request.AllSellers ? await FindSellerIdsAsync(request.Keyword) : request.SellerIds;
            if (sellerIds is { Count: 0 })
            {
                return [];
            }

            var criteria = ExType<SearchSellerCommissionsQuery>.New();
            criteria.CommissionFeeId = request.CommissionFeeId;
            criteria.SellerIds = sellerIds;
            criteria.Take = SearchPageSize;

            return await _sellerCommissionSearchService.SearchAllAsync(criteria);
        }

        /// <summary>
        /// The vendors whose names match the keyword, as SearchSellerCommissionsQueryHandler finds them.
        /// Returns null when there is no keyword, which means all vendors.
        /// </summary>
        protected virtual async Task<IList<string>> FindSellerIdsAsync(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return null;
            }

            var sellersQuery = ExType<SearchSellersQuery>.New();
            sellersQuery.Keyword = keyword;
            var sellers = await _sellersSearchService.SearchAllNoCloneAsync(sellersQuery);

            return sellers.Select(x => x.Id).ToList();
        }
    }
}
