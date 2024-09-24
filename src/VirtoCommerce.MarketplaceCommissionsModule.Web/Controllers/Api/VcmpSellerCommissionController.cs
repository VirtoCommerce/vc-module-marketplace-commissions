using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Commands;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Queries;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;
using VirtoCommerce.MarketplaceVendorModule.Data.Authorization;
using ModuleConstants = VirtoCommerce.MarketplaceVendorModule.Core.ModuleConstants;

namespace VirtoCommerce.MarketplaceCommissionsModule.Web.Controllers.Api
{
    [ApiController]
    [Route("api/vcmp/seller")]
    public class VcmpSellerCommissionController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAuthorizationService _authorizationService;

        public VcmpSellerCommissionController(IMediator mediator, IAuthorizationService authorizationService)
        {
            _mediator = mediator;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        [Route("{sellerId}/commissions")]
        public async Task<ActionResult<CommissionFee>> GetSellerCommission([FromRoute] string sellerId)
        {
            var query = ExType<GetSellerCommissionQuery>.New();
            query.SellerId = sellerId;
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, query, new SellerAuthorizationRequirement(ModuleConstants.Security.Permissions.SellerResources));
            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpPost]
        [Route("commissions")]
        public async Task<ActionResult<CommissionFee>> UpdateSellerCommission([FromBody] UpdateSellerCommissionCommand command)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, command, new SellerAuthorizationRequirement(ModuleConstants.Security.Permissions.SellerResources));
            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
