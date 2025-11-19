using giaoanpro_backend.Application.DTOs.Requests.Classes;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[Authorize]
	public class ClassesController : BaseApiController
	{
		private readonly IClassService _classService;

		public ClassesController(IClassService classService)
		{
			_classService = classService;
		}

		[HttpGet]
		public async Task<IActionResult> GetClasses([FromQuery] GetClassesQuery query)
		{
			var result = await _classService.GetClassesAsync(query);
			return HandleResponse(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetClassById(Guid id)
		{
			var result = await _classService.GetClassByIdAsync(id);
			return HandleResponse(result);
		}

		[HttpPost]
		public async Task<IActionResult> CreateClass([FromBody] CreateClassRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null) return validation;

			var result = await _classService.CreateClassAsync(request);
			return HandleResponse(result);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateClass(Guid id, [FromBody] UpdateClassRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null) return validation;

			var result = await _classService.UpdateClassAsync(id, request);
			return HandleResponse(result);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteClass(Guid id)
		{
			var result = await _classService.DeleteClassAsync(id);
			return HandleResponse(result);
		}
	}
}
