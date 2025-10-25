using giaoanpro_backend.Application.DTOs.Requests.Subscriptions;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class SubscriptionsController : ControllerBase
	{
		private readonly ISubscriptionService _subscriptionService;

		private Guid GetCurrentUserId()
		{
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
		}

		public SubscriptionsController(ISubscriptionService subscriptionService)
		{
			_subscriptionService = subscriptionService;
		}

		[HttpGet("my-current-access")]
		public async Task<IActionResult> GetMyCurrentAccessSubscription()
		{
			var userId = GetCurrentUserId();
			var result = await _subscriptionService.GetCurrentAccessSubscriptionByUserIdAsync(userId);

			return result.Success ? Ok(result) : NotFound(result);
		}

		[HttpGet("my-history")]
		public async Task<IActionResult> GetMySubscriptionHistory()
		{
			var userId = GetCurrentUserId();
			var result = await _subscriptionService.GetSubscriptionHistoryByUserIdAsync(userId);
			return Ok(result);
		}

		[HttpGet("{id:guid}")]
		public async Task<IActionResult> GetMySubscriptionById([FromRoute] Guid id)
		{
			var userId = GetCurrentUserId();
			var result = await _subscriptionService.GetUserSubscriptionByIdAsync(id, userId);

			return result.Success ? Ok(result) : NotFound(result);
		}

		[HttpPost("checkout")]
		public async Task<IActionResult> CreateSubscriptionCheckoutSession([FromBody] SubscriptionCheckoutRequest request)
		{
			request.UserId = GetCurrentUserId();

			var result = await _subscriptionService.CreateSubscriptionCheckoutSessionAsync(request, HttpContext);

			return result.Success ? Ok(result) : BadRequest(result);
		}

		[HttpPost("{id:guid}/cancel")]
		public async Task<IActionResult> CancelSubscription([FromRoute] Guid id)
		{
			var userId = GetCurrentUserId();
			var result = await _subscriptionService.CancelSubscriptionAsync(id, userId);

			return result.Success ? Ok(result) : NotFound(result);
		}
	}
}
