using giaoanpro_backend.Application.DTOs.Requests.Classes;
using giaoanpro_backend.Application.DTOs.Requests.Enrollments;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class ClassEnrollmentsController : BaseApiController
	{
		private readonly IClassEnrollmentService _classEnrollmentService;

		public ClassEnrollmentsController(IClassEnrollmentService classEnrollmentService)
		{
			_classEnrollmentService = classEnrollmentService;
		}

		[HttpGet]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> GetEnrolledClassesForStudent([FromQuery] GetEnrolledClassQuery query)
		{
			var studentId = GetCurrentUserId();
			var result = await _classEnrollmentService.GetEnrolledClassesByStudentIdAsync(studentId, query);
			return HandleResponse(result);
		}

		[HttpPost("enroll")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> EnrollStudentToClass([FromBody] EnrollClassRequest request)
		{
			var studentId = GetCurrentUserId();
			var result = await _classEnrollmentService.EnrollStudentToClassAsync(request.ClassId, studentId);
			return HandleResponse(result);
		}

		[HttpDelete("remove")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> RemoveStudentFromClass([FromBody] EnrollClassRequest request)
		{
			var studentId = GetCurrentUserId();
			var result = await _classEnrollmentService.RemoveStudentFromClassAsync(request.ClassId, studentId);
			return HandleResponse(result);
		}

		[HttpGet("teacher/remove")]
		[Authorize(Roles = "Teacher")]
		public async Task<IActionResult> RemoveStudentFromClassByTeacher([FromQuery] EnrollClassRequest request)
		{
			var result = await _classEnrollmentService.RemoveStudentFromClassAsync(request.ClassId, request.StudentId ?? Guid.Empty);
			return HandleResponse(result);
		}
	}
}
