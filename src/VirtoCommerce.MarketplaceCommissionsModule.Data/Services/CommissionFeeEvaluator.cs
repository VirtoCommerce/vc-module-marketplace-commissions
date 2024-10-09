using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceVendorModule.Data.Integrations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Services
{
    public class CommissionFeeEvaluator : ICommissionFeeEvaluator
    {
        private readonly ICommissionFeeSearchService _searchService;
        private readonly ISellerCommissionCrudService _sellerCommissionCrudService;
        private readonly ISellerResolver _sellerByProductIdResolver;
        private readonly ICommissionFeeResolver _commissionFeeResolver;
        private readonly ILogger _logger;

        public CommissionFeeEvaluator(
            ICommissionFeeSearchService searchService,
            ISellerCommissionCrudService sellerCommissionCrudService,
            ISellerResolver sellerByProductIdResolver,
            ICommissionFeeResolver commissionFeeResolver,
            ILogger<CommissionFeeEvaluator> logger)
        {
            _sellerByProductIdResolver = sellerByProductIdResolver;
            _sellerCommissionCrudService = sellerCommissionCrudService;
            _searchService = searchService;
            _commissionFeeResolver = commissionFeeResolver;
            _logger = logger;
        }

        public virtual async Task<EntryFee[]> EvaluateFeeAsync(CommissionFeeEvaluationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            var result = new Dictionary<string, EntryFee>().WithDefaultValue(null);
            var productIdSellerMap = await _sellerByProductIdResolver.ResolveByProductIds(context.AllEntries.Select(x => x.ProductId).ToArray());
            var commissionFees = await _searchService.SearchAsync(new SearchCommissionFeesQuery { Type = CommissionFeeType.Dynamic, IsActive = true });
            var dynamicCommissionFees = commissionFees.Results.OfType<DynamicCommissionFee>().ToList();
            var sellerIds = productIdSellerMap.Values.Select(x => x.Id).Distinct().ToList();
            var sellerCommissionsMap = await _commissionFeeResolver.ResolveBySellerIds(sellerIds);

            foreach (var entry in context.AllEntries)
            {
                var resultEntry = entry.Clone() as EntryFee;
                var seller = productIdSellerMap[entry.ProductId];
                resultEntry.SellerId = seller?.Id;
                if (!string.IsNullOrEmpty(resultEntry.SellerId) && sellerCommissionsMap.ContainsKey(resultEntry.SellerId))
                {
                    resultEntry.CommissionFee = sellerCommissionsMap[resultEntry.SellerId];
                }

                var verifiableConditions = new CommissionFeeEvaluationContext { AllEntries = context.AllEntries, Entry = resultEntry };

                var matchedDynCommissionFee = dynamicCommissionFees.OrderByDescending(x => x.Priority)
                                                             .FirstOrDefault(x => x.ExpressionTree.IsSatisfiedBy(verifiableConditions));
                if (matchedDynCommissionFee != null)
                {
                    resultEntry.CommissionFee = matchedDynCommissionFee.Clone() as DynamicCommissionFee;
                }

                result[resultEntry.EntryId] = resultEntry;

            }
            return result.Values.ToArray();
        }
    }
}
