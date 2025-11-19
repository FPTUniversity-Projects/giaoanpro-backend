using giaoanpro_backend.Application.DTOs.Requests.Subjects;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[Authorize]
	public class SubjectsController : BaseApiController
	{
		private readonly ISubjectService _subjectService;

		public SubjectsController(ISubjectService subjectService)
		{
			_subjectService = subjectService;
		}

		[HttpGet]
		public async Task<IActionResult> GetSubjects([FromQuery] GetSubjectsQuery query)
		{
			var result = await _subjectService.GetSubjectsAsync(query);
			return HandleResponse(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetSubjectById(Guid id)
		{
			var result = await _subjectService.GetSubjectByIdAsync(id);
			return HandleResponse(result);
		}

		[HttpPost]
		public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null) return validation;

			var result = await _subjectService.CreateSubjectAsync(request);
			return HandleResponse(result);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateSubject(Guid id, [FromBody] UpdateSubjectRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null) return validation;

			var result = await _subjectService.UpdateSubjectAsync(id, request);
			return HandleResponse(result);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteSubject(Guid id)
		{
			var result = await _subjectService.DeleteSubjectAsync(id);
			return HandleResponse(result);
		}
	}
}
