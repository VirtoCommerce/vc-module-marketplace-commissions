using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Services;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.Commands
{
    public class UpdateSellerCommissionCommandHandler : ICommandHandler<UpdateSellerCommissionCommand>
    {
        private readonly ISellerCommissionCrudService _sellerCommissionCrudService;

        public UpdateSellerCommissionCommandHandler(ISellerCommissionCrudService sellerCommissionCrudService)
        {
            _sellerCommissionCrudService = sellerCommissionCrudService;
        }

        public virtual async Task<Unit> Handle(UpdateSellerCommissionCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.SellerId))
            {
                var sellerCommission = await _sellerCommissionCrudService.GetSellerCommission(request.SellerId);
                if (sellerCommission == null)
                {
                    sellerCommission = ExType<SellerCommission>.New();
                    sellerCommission.SellerId = request.SellerId;
                }
                sellerCommission.CommissionFeeId = request.CommissionFeeId;

                await _sellerCommissionCrudService.SaveChangesAsync([sellerCommission]);
            }


            return Unit.Value;
        }
    }
}
