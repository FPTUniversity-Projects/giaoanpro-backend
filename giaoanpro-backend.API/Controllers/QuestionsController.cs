using giaoanpro_backend.Application.DTOs.Requests.Questions;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles ="Teacher")]
	public class QuestionsController : BaseApiController
	{
		private readonly IQuestionService _questionService;

		public QuestionsController(IQuestionService questionService)
		{
			_questionService = questionService;
		}

		[HttpGet]
		public async Task<IActionResult> GetAllQuestions([FromQuery] GetQuestionsRequest request)
		{
			var userId = GetCurrentUserId();
			var result = await _questionService.GetAllQuestionsAsync(request, userId);
			return HandleResponse(result);
		}

		[HttpGet("{id:Guid}")]
		public async Task<IActionResult> GetQuestionById(Guid id)
		{
			var result = await _questionService.GetQuestionByIdAsync(id);
			return HandleResponse(result);
		}

		[HttpPost]
		public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null)
			{
				return validation;
			}

			var userId = GetCurrentUserId();
			var result = await _questionService.CreateQuestionAsync(request, userId);
			return HandleResponse(result);
		}

		[HttpPost("bulk")]
		public async Task<IActionResult> CreateQuestionsBulk([FromBody] List<CreateQuestionRequest> requests)
		{
			var validation = ValidateRequestBody(requests);
			if (validation != null)
			{
				return validation;
			}

			var userId = GetCurrentUserId();
			var result = await _questionService.CreateQuestionsBulkAsync(requests, userId);
			return HandleResponse(result);
		}

		[HttpPost("generate-ai")]
		public async Task<IActionResult> GenerateQuestionsAi([FromBody] GenerateQuestionsRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null)
			{
				return validation;
			}

			var userId = GetCurrentUserId();
			var result = await _questionService.GenerateQuestionsAiAsync(request, userId);
			return HandleResponse(result);
		}

		[HttpPut("{id:Guid}")]
		public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] UpdateQuestionRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null)
			{
				return validation;
			}

			var result = await _questionService.UpdateQuestionAsync(id, request);
			return HandleResponse(result);
		}

		[HttpDelete("{id:Guid}")]
		public async Task<IActionResult> DeleteQuestion(Guid id)
		{
			var result = await _questionService.DeleteQuestionAsync(id);
			return HandleResponse(result);
		}

		[HttpGet("export-pdf/{lessonPlanId:Guid}")]
		public async Task<IActionResult> ExportQuestionsPdf(Guid lessonPlanId, [FromQuery] GetQuestionsRequest? filterRequest = null)
		{
			try
			{
				var userId = GetCurrentUserId();
				var pdfBytes = await _questionService.ExportQuestionsPdfAsync(lessonPlanId, filterRequest, userId);
				return File(pdfBytes, "application/pdf", $"questions-{lessonPlanId}.pdf");
			}
			catch (UnauthorizedAccessException ex)
			{
				var forbiddenResponse = BaseResponse<string>.Fail(ex.Message, ResponseErrorType.Forbidden);
				return HandleResponse(forbiddenResponse);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = $"Không thể xuất PDF: {ex.Message}" });
			}
		}
	}
}
