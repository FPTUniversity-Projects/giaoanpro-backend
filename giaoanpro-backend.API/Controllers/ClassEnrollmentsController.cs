using giaoanpro_backend.Application.DTOs.Requests.Classes;
using giaoanpro_backend.Application.DTOs.Requests.Enrollments;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = "Student")]
	public class ClassEnrollmentsController : BaseApiController
	{
		private readonly IClassEnrollmentService _classEnrollmentService;

		public ClassEnrollmentsController(IClassEnrollmentService classEnrollmentService)
		{
			_classEnrollmentService = classEnrollmentService;
		}

		[HttpGet]
		public async Task<IActionResult> GetEnrolledClassesForStudent([FromQuery] GetEnrolledClassQuery query)
		{
			var studentId = GetCurrentUserId();
			var result = await _classEnrollmentService.GetEnrolledClassesByStudentIdAsync(studentId, query);
			return HandleResponse(result);
		}

		[HttpPost("enroll")]
		public async Task<IActionResult> EnrollStudentToClass([FromBody] EnrollClassRequest request)
		{
			var studentId = GetCurrentUserId();
			var result = await _classEnrollmentService.EnrollStudentToClassAsync(request.ClassId, studentId);
			return HandleResponse(result);
		}

		[HttpDelete("remove")]
		public async Task<IActionResult> RemoveStudentFromClass([FromBody] EnrollClassRequest request)
		{
			var studentId = GetCurrentUserId();
			var result = await _classEnrollmentService.RemoveStudentFromClassAsync(request.ClassId, studentId);
			return HandleResponse(result);
		}
	}
}
