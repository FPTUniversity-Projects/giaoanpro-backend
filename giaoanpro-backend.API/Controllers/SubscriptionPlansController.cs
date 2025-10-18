using giaoanpro_backend.Application.DTOs.Requests.SubscriptionPlans;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SubscriptionPlansController : ControllerBase
	{
		private readonly ISubscriptionPlanService _subscriptionPlanService;

		public SubscriptionPlansController(ISubscriptionPlanService subscriptionPlanService)
		{
			_subscriptionPlanService = subscriptionPlanService;
		}

		[HttpGet]
		public async Task<IActionResult> GetAllSubscriptionPlans()
		{
			var result = await _subscriptionPlanService.GetAllSubscriptionPlansAsync();
			return result.Success ? Ok(result) : BadRequest(result);
		}

		[HttpGet("{id:Guid}")]
		public async Task<IActionResult> GetSubscriptionPlanById(Guid id)
		{
			var result = await _subscriptionPlanService.GetSubscriptionPlanByIdAsync(id);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		[HttpPost]
		public async Task<IActionResult> CreateSubscriptionPlan([FromBody] CreateSubscriptionPlanRequest request)
		{
			var result = await _subscriptionPlanService.CreateSubscriptionPlanAsync(request);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		[HttpPut("{id:Guid}")]
		public async Task<IActionResult> UpdateSubscriptionPlan(Guid id, [FromBody] UpdateSubscriptionPlanRequest request)
		{
			var result = await _subscriptionPlanService.UpdateSubscriptionPlanAsync(id, request);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		[HttpDelete("{id:Guid}")]
		public async Task<IActionResult> DeleteSubscriptionPlan(Guid id)
		{
			var result = await _subscriptionPlanService.DeleteSubscriptionPlanAsync(id);
			return result.Success ? Ok(result) : BadRequest(result);
		}
	}
}
