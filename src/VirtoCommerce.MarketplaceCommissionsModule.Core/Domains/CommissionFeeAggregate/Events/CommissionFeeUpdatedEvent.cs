using MediatR;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core.Domains
{
    public class CommissionFeeUpdatedEvent : INotification
    {
        public CommissionFee CommissionFee { get; set; }
        public CommissionFee OldValue { get; set; }
    }
}
