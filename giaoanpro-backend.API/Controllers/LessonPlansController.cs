using giaoanpro_backend.Application.DTOs.Requests.LessonPlans;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonPlansController : BaseApiController
    {
        private readonly ILessonPlanService _lessonPlanService;

        public LessonPlansController(ILessonPlanService lessonPlanService)
        {
            _lessonPlanService = lessonPlanService;
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetLessonPlans([FromQuery] GetLessonPlansQuery query)
        {
            var userId = GetCurrentUserId();
            var result = await _lessonPlanService.GetLessonPlansAsync(query, userId);
            return HandleResponse(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLessonPlanById(Guid id)
        {
            var result = await _lessonPlanService.GetLessonPlanByIdAsync(id);
            return HandleResponse(result);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateLessonPlan([FromBody] CreateLessonPlanRequest request)
        {
            var validation = ValidateRequestBody(request);
            if (validation != null) return validation;

            var userId = GetCurrentUserId();
            var result = await _lessonPlanService.CreateLessonPlanAsync(request, userId);
            return HandleResponse(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateLessonPlan(Guid id, [FromBody] UpdateLessonPlanRequest request)
        {
            var validation = ValidateRequestBody(request);
            if (validation != null) return validation;

            var userId = GetCurrentUserId();
            var result = await _lessonPlanService.UpdateLessonPlanAsync(id, request, userId);
            return HandleResponse(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteLessonPlan(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _lessonPlanService.DeleteLessonPlanAsync(id, userId);
            return HandleResponse(result);
        }
    }
}
