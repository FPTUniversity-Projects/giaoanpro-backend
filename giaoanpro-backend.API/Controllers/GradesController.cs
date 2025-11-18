using giaoanpro_backend.Application.DTOs.Requests.Grades;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[Authorize]
	public class GradesController : BaseApiController
	{
		private readonly IGradeService _gradeService;

		public GradesController(IGradeService gradeService)
		{
			_gradeService = gradeService;
		}

		[HttpGet]
		public async Task<IActionResult> GetGrades([FromQuery] GetGradesQuery query)
		{
			var result = await _gradeService.GetGradesAsync(query);
			return HandleResponse(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetGradeById(Guid id)
		{
			var result = await _gradeService.GetGradeByIdAsync(id);
			return HandleResponse(result);
		}

		[HttpPost]
		public async Task<IActionResult> CreateGrade([FromBody] CreateGradeRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null) return validation;

			var result = await _gradeService.CreateGradeAsync(request);
			return HandleResponse(result);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateGrade(Guid id, [FromBody] UpdateGradeRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null) return validation;

			var result = await _gradeService.UpdateGradeAsync(id, request);
			return HandleResponse(result);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteGrade(Guid id)
		{
			var result = await _gradeService.DeleteGradeAsync(id);
			return HandleResponse(result);
		}
	}
}
