using giaoanpro_backend.Application.DTOs.Requests.Subscriptions;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Subscriptions;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class SubscriptionsController : BaseApiController
	{
		private readonly ISubscriptionService _subscriptionService;

		public SubscriptionsController(ISubscriptionService subscriptionService)
		{
			_subscriptionService = subscriptionService;
		}

		[HttpGet("my-current-access")]
		[ProducesResponseType(typeof(BaseResponse<GetSubscriptionResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<GetSubscriptionResponse>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<BaseResponse<GetSubscriptionResponse>>> GetMyCurrentAccessSubscription()
		{
			var userId = GetCurrentUserId();
			var result = await _subscriptionService.GetCurrentAccessSubscriptionByUserIdAsync(userId);

			return HandleResponse(result);
		}

		[HttpGet("my-history")]
		[ProducesResponseType(typeof(BaseResponse<List<GetHistorySubscriptionResponse>>), StatusCodes.Status200OK)]
		public async Task<ActionResult<BaseResponse<List<GetHistorySubscriptionResponse>>>> GetMySubscriptionHistory()
		{
			var userId = GetCurrentUserId();
			var result = await _subscriptionService.GetSubscriptionHistoryByUserIdAsync(userId);
			return HandleResponse(result);
		}

		[HttpGet("{id:guid}")]
		[ProducesResponseType(typeof(BaseResponse<GetSubscriptionDetailResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<GetSubscriptionDetailResponse>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<BaseResponse<GetSubscriptionDetailResponse>>> GetMySubscriptionById([FromRoute] Guid id)
		{
			var userId = GetCurrentUserId();
			var result = await _subscriptionService.GetUserSubscriptionByIdAsync(id, userId);

			return HandleResponse(result);
		}

		[HttpPost("checkout")]
		[ProducesResponseType(typeof(BaseResponse<SubscriptionCheckoutResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<SubscriptionCheckoutResponse>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(BaseResponse<SubscriptionCheckoutResponse>), StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(BaseResponse<SubscriptionCheckoutResponse>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<SubscriptionCheckoutResponse>>> CreateSubscriptionCheckoutSession([FromBody] SubscriptionCheckoutRequest request)
		{
			var userId = GetCurrentUserId();
			var result = await _subscriptionService.CreateSubscriptionCheckoutSessionAsync(userId, request, HttpContext);

			return HandleResponse(result);
		}

		[HttpPost("{id:guid}/cancel")]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<string>>> CancelSubscription([FromRoute] Guid id)
		{
			var userId = GetCurrentUserId();
			var result = await _subscriptionService.CancelSubscriptionAsync(id, userId);

			return HandleResponse(result);
		}
	}
}
