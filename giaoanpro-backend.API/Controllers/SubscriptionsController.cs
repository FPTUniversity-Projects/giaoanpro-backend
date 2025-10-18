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

		[HttpPost("checkout")]
		public async Task<IActionResult> CreateSubscriptionCheckoutSession([FromBody] SubscriptionCheckoutRequest request)
		{
			var result = await _subscriptionService.CreateSubscriptionCheckoutSessionAsync(request, HttpContext);
			return result.Success ? Ok(result) : BadRequest(result);
		}
	}
}
