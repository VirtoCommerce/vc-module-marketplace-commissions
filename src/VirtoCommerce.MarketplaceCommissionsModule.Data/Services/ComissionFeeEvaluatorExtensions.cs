using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Services
{
    public static class ComissionFeeEvaluatorExtensions
    {

        public static async Task EvalAndApplyFees(this ICommissionFeeEvaluator evaluator, CustomerOrder order, ICategoryService categoryService)
        {
            //Create eval context
            var categoryIds = order.Items.Select(x => x.CategoryId);
            var categories = await categoryService.GetAsync(categoryIds.ToArray());

            var feeEntries = order.Items.Select(x => new EntryFee
            {
                CategoryId = x.CategoryId,
                Outline = categories.FirstOrDefault(y => y.Id == x.CategoryId).Outline,
                EntryId = x.Id,
                EntryType = nameof(LineItem),
                Price = x.ExtendedPrice,
                PriceWithTax = x.ExtendedPriceWithTax,
                ProductId = x.ProductId,
                Quantity = x.Quantity
            }).ToArray();

            var context = new CommissionFeeEvaluationContext
            {
                AllEntries = feeEntries
            };

            //eval
            feeEntries = await evaluator.EvaluateFeeAsync(context);

            //apply
            foreach (var lineItem in order.Items)
            {
                var entryFee = feeEntries.FirstOrDefault(x => x.EntryId == lineItem.Id);
                if (entryFee != null && entryFee.CommissionFee != null)
                {
                    var feeAmount = entryFee.CommissionFee.GetFeeAmount(lineItem.ExtendedPrice);
                    lineItem.Fee = feeAmount;
                    lineItem.FeeWithTax = entryFee.CommissionFee.GetFeeAmount(lineItem.ExtendedPriceWithTax);
                    if (lineItem.FeeDetails == null)
                    {
                        lineItem.FeeDetails = new List<FeeDetail>();
                    }
                    if (order.FeeDetails == null)
                    {
                        order.FeeDetails = new List<FeeDetail>();
                    }
                    var feeDetail = new FeeDetail
                    {
                        Amount = feeAmount,
                        Currency = lineItem.Currency,
                        Description = $"{entryFee.CommissionFee.Name} {entryFee.CommissionFee.Description}",
                        FeeId = entryFee.CommissionFee.Id
                    };
                    lineItem.FeeDetails.Add(feeDetail);
                    order.FeeDetails.Add(feeDetail);
                }
            }
        }
    }
}
