using giaoanpro_backend.Application.DTOs.Requests.Semesters;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[Authorize]
	public class SemestersController : BaseApiController
	{
		private readonly ISemesterService _semesterService;

		public SemestersController(ISemesterService semesterService)
		{
			_semesterService = semesterService;
		}

		[HttpGet]
		public async Task<IActionResult> GetSemesters([FromQuery] GetSemestersQuery query)
		{
			var result = await _semesterService.GetSemestersAsync(query);
			return HandleResponse(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetSemesterById(Guid id)
		{
			var result = await _semesterService.GetSemesterByIdAsync(id);
			return HandleResponse(result);
		}

		[HttpPost]
		public async Task<IActionResult> CreateSemester([FromBody] CreateSemesterRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null) return validation;

			var result = await _semesterService.CreateSemesterAsync(request);
			return HandleResponse(result);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateSemester(Guid id, [FromBody] UpdateSemesterRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null) return validation;

			var result = await _semesterService.UpdateSemesterAsync(id, request);
			return HandleResponse(result);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteSemester(Guid id)
		{
			var result = await _semesterService.DeleteSemesterAsync(id);
			return HandleResponse(result);
		}
	}
}
