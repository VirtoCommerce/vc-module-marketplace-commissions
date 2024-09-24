using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Domains;
using VirtoCommerce.MarketplaceCommissionsModule.Core.Models.Search;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Commands;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Queries;
using VirtoCommerce.MarketplaceVendorModule.Core.Common;
using ModuleConstants = VirtoCommerce.MarketplaceVendorModule.Core.ModuleConstants;

namespace VirtoCommerce.MarketplaceCommissionsModule.Web.Controllers.Api
{
    [ApiController]
    [Route("api/vcmp/commissions")]
    [Authorize(ModuleConstants.Security.Permissions.OperatorResources)]

    public class VcmpFeeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VcmpFeeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("new")]
        public ActionResult<DynamicCommissionFee> GetNewFee()
        {
            var result = ExType<DynamicCommissionFee>.New();
            result.Type = CommissionFeeType.Dynamic;
            result.CalculationType = FeeCalculationType.Percent;
            result.ExpressionTree = ExType<DynamicCommissionFeeTree>.New();
            result.ExpressionTree.MergeFromPrototype(ExType<DynamicCommissionFeeTreePrototype>.New());
            result.IsActive = true;

            return Ok(result);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<CommissionFee>> GetFeeById([FromRoute] string id)
        {
            var query = ExType<GetFeesQuery>.New();
            query.Ids = new[] { id };
            var queryResult = await _mediator.Send(query);

            var result = queryResult.FirstOrDefault();
            if (result is DynamicCommissionFee dynamicCommissionFee)
            {
                dynamicCommissionFee.ExpressionTree?.MergeFromPrototype(ExType<DynamicCommissionFeeTreePrototype>.New());
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<CommissionFee>> CreateFee([FromBody] CreateFeeCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult<CommissionFee>> UpdateFee([FromBody] UpdateFeeCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteFee([FromQuery] string[] ids)
        {
            var commands = ids.Select(x => new DeleteFeeCommand { Id = x }).ToArray();
            foreach (var command in commands)
            {
                await _mediator.Send(command);
            }

            return Ok();
        }

        [HttpPost]
        [Route("search")]
        public async Task<ActionResult<SearchCommissionFeesResult>> SearchFee([FromBody] SearchCommissionFeesQuery query)
        {
            var result = await _mediator.Send(query);

            return Ok(result);
        }
    }
}
