using giaoanpro_backend.Application.DTOs.Requests.Subscriptions;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SubscriptionsController : ControllerBase
	{
		private readonly ISubscriptionService _subscriptionService;

		public SubscriptionsController(ISubscriptionService subscriptionService)
		{
			_subscriptionService = subscriptionService;
		}

		[HttpGet("user/{userId:guid}/subscriptions/current-access")]
		public async Task<IActionResult> GetCurrentAccessSubscriptionByUserId([FromRoute] Guid userId)
		{
			var result = await _subscriptionService.GetCurrentAccessSubscriptionByUserIdAsync(userId);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		[HttpGet("user/{userId:guid}/subscriptions/history")]
		public async Task<IActionResult> GetSubscriptionHistoryByUserId([FromRoute] Guid userId)
		{
			var result = await _subscriptionService.GetSubscriptionHistoryByUserIdAsync(userId);
			return Ok(result);
		}

		[HttpGet("user/{userId:guid}/subscriptions/{id:guid}")]
		public async Task<IActionResult> GetSubscriptionById([FromRoute] Guid id, Guid userId)
		{
			var result = await _subscriptionService.GetUserSubscriptionByIdAsync(id, userId);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		[HttpPost("checkout")]
		public async Task<IActionResult> CreateSubscriptionCheckoutSession([FromBody] SubscriptionCheckoutRequest request)
		{
			var result = await _subscriptionService.CreateSubscriptionCheckoutSessionAsync(request, HttpContext);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		[HttpPost("user/{userId:guid}/cancel/{subscriptionId:guid}")]
		public async Task<IActionResult> CancelSubscription([FromRoute] Guid subscriptionId, Guid userId)
		{
			var result = await _subscriptionService.CancelSubscriptionAsync(subscriptionId, userId);
			return result.Success ? Ok(result) : BadRequest(result);
		}
	}
}
