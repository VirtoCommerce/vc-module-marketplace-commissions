using System;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Domains
{
    public class DynamicCommissionFee : CommissionFee
    {
        public bool IsActive { get; set; }
        public DynamicCommissionFeeTree ExpressionTree { get; set; }

        public override void SetAsDefault(CommissionFee previousDefault)
        {
            throw new InvalidOperationException("Unable to set as default for Dynamic fee, use priority instead");
        }

        public override void Update(CommissionFeeDetails commissionFee)
        {
            base.Update(commissionFee);
            IsActive = commissionFee.IsActive;
            ExpressionTree = commissionFee.ExpressionTree;
        }

        public new static DynamicCommissionFee CreateNew(CommissionFeeDetails commissionFee)
        {
            var result = new DynamicCommissionFee
            {
                Name = commissionFee.Name,
                Description = commissionFee.Description,
                Type = commissionFee.Type,
                CalculationType = commissionFee.CalculationType,
                Fee = commissionFee.Fee,
                Priority = commissionFee.Priority,
                IsActive = commissionFee.IsActive,
                ExpressionTree = commissionFee.ExpressionTree
            };

            return result;
        }
    }
}
