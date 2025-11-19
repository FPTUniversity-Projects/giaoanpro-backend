using giaoanpro_backend.Application.DTOs.Requests.Activities;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ActivitiesController : BaseApiController
	{
		private readonly IActivityService _activityService;

		public ActivitiesController(IActivityService activityService)
		{
			_activityService = activityService;
		}

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetActivities([FromQuery] GetActivitiesQuery query)
        {
            var userId = GetCurrentUserId();
            var result = await _activityService.GetActivitiesAsync(query, userId);
            return HandleResponse(result);
        }

		[HttpGet("{id}")]
		public async Task<IActionResult> GetActivityById(Guid id)
		{
			var result = await _activityService.GetActivityByIdAsync(id);
			return HandleResponse(result);
		}

		[HttpPost]
		[Authorize(Roles = "Teacher")]
		public async Task<IActionResult> CreateActivity([FromBody] CreateActivityRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null) return validation;

			var result = await _activityService.CreateActivityAsync(request);
			return HandleResponse(result);
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "Teacher")]
		public async Task<IActionResult> UpdateActivity(Guid id, [FromBody] UpdateActivityRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null) return validation;

            var userId = GetCurrentUserId();
            var result = await _activityService.UpdateActivityAsync(id, request, userId);
            return HandleResponse(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteActivity(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _activityService.DeleteActivityAsync(id, userId);
            return HandleResponse(result);
        }
    }
}
