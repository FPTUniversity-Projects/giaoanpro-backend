using giaoanpro_backend.Application.DTOs.Requests.SubscriptionPlans;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.SubscriptionPlans;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SubscriptionPlansController : BaseApiController
	{
		private readonly ISubscriptionPlanService _subscriptionPlanService;

		public SubscriptionPlansController(ISubscriptionPlanService subscriptionPlanService)
		{
			_subscriptionPlanService = subscriptionPlanService;
		}

		[HttpGet]
		[ProducesResponseType(typeof(BaseResponse<PagedResult<GetSubscriptionPlanResponse>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<PagedResult<GetSubscriptionPlanResponse>>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<PagedResult<GetSubscriptionPlanResponse>>>> GetSubscriptionPlans([FromQuery] GetSubscriptionPlansQuery query)
		{
			var result = await _subscriptionPlanService.GetSubscriptionPlansAsync(query);
			return HandleResponse(result);
		}

		[HttpGet("{id:Guid}")]
		[ProducesResponseType(typeof(BaseResponse<GetSubscriptionPlanResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<GetSubscriptionPlanResponse>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(BaseResponse<GetSubscriptionPlanResponse>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<GetSubscriptionPlanResponse>>> GetSubscriptionPlanById(Guid id)
		{
			var result = await _subscriptionPlanService.GetSubscriptionPlanByIdAsync(id);
			return HandleResponse(result);
		}

		[HttpGet("lookup")]
		public async Task<ActionResult<BaseResponse<List<SubscriptionPlanLookupResponse>>>> GetSubscriptionPlanLookups()
		{
			var result = await _subscriptionPlanService.GetSubscriptionPlanLookupsAsync();
			return HandleResponse(result);
		}

		[HttpGet("admin/lookup")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<BaseResponse<List<SubscriptionPlanLookupResponse>>>> GetSubscriptionPlanAdminLookups([FromQuery] bool isActiveOnly = true)
		{
			var result = await _subscriptionPlanService.GetSubscriptionPlanAdminLookupsAsync(isActiveOnly);
			return HandleResponse(result);
		}

		[HttpPost]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<string>>> CreateSubscriptionPlan([FromBody] CreateSubscriptionPlanRequest request)
		{
			var result = await _subscriptionPlanService.CreateSubscriptionPlanAsync(request);
			return HandleResponse(result);
		}

		[HttpPut("{id:Guid}")]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<string>>> UpdateSubscriptionPlan(Guid id, [FromBody] UpdateSubscriptionPlanRequest request)
		{
			var result = await _subscriptionPlanService.UpdateSubscriptionPlanAsync(id, request);
			return HandleResponse(result);
		}

		[HttpDelete("{id:Guid}")]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<string>>> DeleteSubscriptionPlan(Guid id)
		{
			var result = await _subscriptionPlanService.DeleteSubscriptionPlanAsync(id);
			return HandleResponse(result);
		}
	}
}
