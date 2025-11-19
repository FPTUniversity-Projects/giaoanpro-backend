using giaoanpro_backend.Application.DTOs.Requests.Syllabuses;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[Authorize]
	public class SyllabusesController : BaseApiController
	{
		private readonly ISyllabusService _syllabusService;

		public SyllabusesController(ISyllabusService syllabusService)
		{
			_syllabusService = syllabusService;
		}

		[HttpGet]
		public async Task<IActionResult> GetSyllabuses([FromQuery] GetSyllabusesQuery query)
		{
			var result = await _syllabusService.GetSyllabusesAsync(query);
			return HandleResponse(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetSyllabusById(Guid id)
		{
			var result = await _syllabusService.GetSyllabusByIdAsync(id);
			return HandleResponse(result);
		}

		[HttpGet("by-subject/{subjectId}")]
		public async Task<IActionResult> GetSyllabusBySubjectId(Guid subjectId)
		{
			var result = await _syllabusService.GetSyllabusBySubjectIdAsync(subjectId);
			return HandleResponse(result);
		}

		[HttpPost]
		public async Task<IActionResult> CreateSyllabus([FromBody] CreateSyllabusRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null) return validation;

			var result = await _syllabusService.CreateSyllabusAsync(request);
			return HandleResponse(result);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateSyllabus(Guid id, [FromBody] UpdateSyllabusRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null) return validation;

			var result = await _syllabusService.UpdateSyllabusAsync(id, request);
			return HandleResponse(result);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteSyllabus(Guid id)
		{
			var result = await _syllabusService.DeleteSyllabusAsync(id);
			return HandleResponse(result);
		}
	}
}
