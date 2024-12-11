using System;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Domains
{
    public class CommissionFee : AggregateRoot
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public CommissionFeeType Type { get; set; }
        public FeeCalculationType CalculationType { get; set; }
        public decimal Fee { get; set; }
        public int Priority { get; set; }
        public bool IsDefault { get; set; }


        public virtual void SetAsDefault(CommissionFee previousDefault)
        {
            if (previousDefault != null && previousDefault.Id != Id)
            {
                previousDefault.IsDefault = false;
            }
            IsDefault = true;
        }

        public virtual void Update(CommissionFeeDetails commissionFee)
        {
            if (commissionFee == null)
            {
                throw new ArgumentNullException(nameof(commissionFee));
            }

            var oldValue = Clone() as CommissionFee;

            Name = commissionFee.Name;
            Description = commissionFee.Description;
            Type = commissionFee.Type;
            CalculationType = commissionFee.CalculationType;
            Fee = commissionFee.Fee;
            Priority = commissionFee.Priority;

            AddDomainEvent(new CommissionFeeUpdatedEvent { CommissionFee = Clone() as CommissionFee, OldValue = oldValue });
        }

        public static CommissionFee CreateNew(CommissionFeeDetails commissionFee)
        {
            if (commissionFee == null)
            {
                throw new ArgumentNullException(nameof(commissionFee));
            }

            var result = new CommissionFee
            {
                Name = commissionFee.Name,
                Description = commissionFee.Description,
                Type = commissionFee.Type,
                CalculationType = commissionFee.CalculationType,
                Fee = commissionFee.Fee,
                Priority = commissionFee.Priority
            };
            return result;
        }

        public virtual decimal GetFeeAmount(decimal price)
        {
            if (price < 0)
            {
                throw new ArgumentNullException($"The {nameof(price)} cannot be negative");
            }

            var result = Math.Min(Fee, price);
            if (CalculationType == FeeCalculationType.Percent)
            {
                result = price * Fee * 0.01m;
            }

            return result;
        }
    }
}
