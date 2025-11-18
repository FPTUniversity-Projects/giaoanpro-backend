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

		[HttpGet("me/current")]
		[ProducesResponseType(typeof(BaseResponse<GetMyCurrentAccessResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<GetMyCurrentAccessResponse>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<BaseResponse<GetMyCurrentAccessResponse>>> GetMyCurrentAccessSubscription()
		{
			var userId = GetCurrentUserId();
			var result = await _subscriptionService.GetCurrentAccessSubscriptionByUserIdAsync(userId);
			return HandleResponse(result);
		}

		[HttpGet("me/history")]
		[ProducesResponseType(typeof(BaseResponse<PagedResult<GetMyHistorySubscriptionResponse>>), StatusCodes.Status200OK)]
		public async Task<ActionResult<BaseResponse<PagedResult<GetMyHistorySubscriptionResponse>>>> GetMySubscriptionHistory([FromQuery] GetMySubscriptionHistoryQuery query)
		{
			var userId = GetCurrentUserId();
			var result = await _subscriptionService.GetSubscriptionHistoryByUserIdAsync(userId, query);
			return HandleResponse(result);
		}

		[HttpGet("me/{id:guid}")]
		[ProducesResponseType(typeof(BaseResponse<GetMySubscriptionDetailResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<GetMySubscriptionDetailResponse>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<BaseResponse<GetMySubscriptionDetailResponse>>> GetMySubscriptionById([FromRoute] Guid id)
		{
			var userId = GetCurrentUserId();
			var result = await _subscriptionService.GetUserSubscriptionByIdAsync(id, userId);
			return HandleResponse(result);
		}

		// --- Admin / public endpoints ---
		[HttpGet]
		[Authorize(Roles = "Admin")]
		[ProducesResponseType(typeof(BaseResponse<PagedResult<GetHistorySubscriptionResponse>>), StatusCodes.Status200OK)]
		public async Task<ActionResult<BaseResponse<PagedResult<GetHistorySubscriptionResponse>>>> GetSubscriptions([FromQuery] GetSubscriptionsQuery query)
		{
			var result = await _subscriptionService.GetSubscriptionsAsync(query);
			return HandleResponse(result);
		}

		[HttpGet("{id:guid}")]
		[Authorize(Roles = "Admin")]
		[ProducesResponseType(typeof(BaseResponse<GetSubscriptionDetailResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<GetSubscriptionDetailResponse>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<BaseResponse<GetSubscriptionDetailResponse>>> GetSubscriptionById([FromRoute] Guid id)
		{
			var result = await _subscriptionService.GetSubscriptionByIdAsync(id);
			return HandleResponse(result);
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<string>>> CreateSubscription([FromBody] CreateSubscriptionRequest request)
		{
			var validator = ValidateRequestBody<string>(request);
			if (validator != null)
			{
				return validator;
			}
			var result = await _subscriptionService.CreateSubscriptionAsync(request);
			return HandleResponse(result);
		}

		[HttpPost("checkout")]
		[ProducesResponseType(typeof(BaseResponse<SubscriptionCheckoutResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<SubscriptionCheckoutResponse>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(BaseResponse<SubscriptionCheckoutResponse>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(BaseResponse<SubscriptionCheckoutResponse>), StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(BaseResponse<SubscriptionCheckoutResponse>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<SubscriptionCheckoutResponse>>> CreateSubscriptionCheckoutSession([FromBody] SubscriptionCheckoutRequest request)
		{
			var validator = ValidateRequestBody<SubscriptionCheckoutResponse>(request);
			if (validator != null)
			{
				return validator;
			}
			var userId = GetCurrentUserId();
			var result = await _subscriptionService.CreateSubscriptionCheckoutSessionAsync(userId, request, HttpContext);
			return HandleResponse(result);
		}

		[HttpPatch("{id:guid}/cancel")]
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

		[HttpPatch("{id:guid}/status")]
		[Authorize(Roles = "Admin")]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<string>>> UpdateSubscriptionStatus([FromRoute] Guid id, [FromBody] UpdateSubscriptionStatusRequest request)
		{
			var validator = ValidateRequestBody<string>(request);
			if (validator != null)
			{
				return validator;
			}
			var result = await _subscriptionService.UpdateSubscriptionStatusAsync(id, request);
			return HandleResponse(result);
		}
	}
}
