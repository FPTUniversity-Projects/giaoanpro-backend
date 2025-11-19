using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using giaoanpro_backend.Application.DTOs.Requests.Exams;
using giaoanpro_backend.Application.DTOs.Responses.Exams;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/exams")]
	[ApiController]
	[Authorize]
	public class ExamsController : BaseApiController
	{
		private readonly IExamService _examService;

		public ExamsController(IExamService examService)
		{
			_examService = examService;
		}

		[HttpPost]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<string>>> CreateExam([FromBody] CreateExamRequest request)
		{
			var validation = ValidateRequestBody<string>(request);
			if (validation != null) return validation;

			var currentUserId = GetCurrentUserId();
			var result = await _examService.CreateExamAsync(request, currentUserId);

			if (!result.Success)
			{
				return HandleResponse(result);
			}

			// On success return 201 Created with Location header
			if (result.Payload != null && Guid.TryParse(result.Payload, out var createdId))
			{
				return CreatedAtAction(nameof(GetExamById), new { id = createdId }, result);
			}

			return StatusCode(201, result);
		}

		[HttpGet("{id:guid}")]
		[ProducesResponseType(typeof(BaseResponse<GetExamDetailResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<GetExamDetailResponse>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(BaseResponse<GetExamDetailResponse>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(BaseResponse<GetExamDetailResponse>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(BaseResponse<GetExamDetailResponse>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<GetExamDetailResponse>>> GetExamById([FromRoute] Guid id)
		{
			var result = await _examService.GetExamByIdAsync(id);
			return HandleResponse(result);
		}

		[HttpGet("inventory")]
		[ProducesResponseType(typeof(BaseResponse<GetExamsPagedResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<GetExamsPagedResponse>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(BaseResponse<GetExamsPagedResponse>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(BaseResponse<GetExamsPagedResponse>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(BaseResponse<GetExamsPagedResponse>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<GetExamsPagedResponse>>> GetTeacherInventory([FromQuery] GetExamInventoryRequest request)
		{
			var currentUserId = GetCurrentUserId();
			var result = await _examService.GetTeacherInventoryAsync(request, currentUserId);
			return HandleResponse(result);
		}

		// PUT /api/exams/{id}
		[HttpPut("{id:guid}")]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<string>>> UpdateExam([FromRoute] Guid id, [FromBody] UpdateExamRequest request)
		{
			var validation = ValidateRequestBody<string>(request);
			if (validation != null) return validation;

			if (id != request.Id)
			{
				var mismatch = BaseResponse<string>.Fail("Route id does not match request id.", ResponseErrorType.BadRequest);
				return HandleResponse(mismatch);
			}

			var currentUserId = GetCurrentUserId();
			var result = await _examService.UpdateExamAsync(request, currentUserId);
			return HandleResponse(result);
		}

		// DELETE /api/exams/{id}
		[HttpDelete("{id:guid}")]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<string>>> DeleteExam([FromRoute] Guid id)
		{
			var currentUserId = GetCurrentUserId();
			var result = await _examService.DeleteExamAsync(id, currentUserId);
			return HandleResponse(result);
		}
	}
}
