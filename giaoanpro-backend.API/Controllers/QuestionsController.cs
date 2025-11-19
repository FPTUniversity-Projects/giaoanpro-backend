using giaoanpro_backend.Application.DTOs.Requests.Questions;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class QuestionsController : ControllerBase
	{
		private readonly IQuestionService _questionService;

		public QuestionsController(IQuestionService questionService)
		{
			_questionService = questionService;
		}

		[HttpGet]
		public async Task<IActionResult> GetAllQuestions([FromQuery] GetQuestionsRequest request)
		{
			var result = await _questionService.GetAllQuestionsAsync(request);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		[HttpGet("{id:Guid}")]
		public async Task<IActionResult> GetQuestionById(Guid id)
		{
			var result = await _questionService.GetQuestionByIdAsync(id);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		[HttpPost]
		public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionRequest request)
		{
			var result = await _questionService.CreateQuestionAsync(request);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		[HttpPost("bulk")]
		public async Task<IActionResult> CreateQuestionsBulk([FromBody] List<CreateQuestionRequest> requests)
		{
			var result = await _questionService.CreateQuestionsBulkAsync(requests);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		[HttpPost("generate-ai")]
		public async Task<IActionResult> GenerateQuestionsAi([FromBody] GenerateQuestionsRequest request)
		{
			var result = await _questionService.GenerateQuestionsAiAsync(request);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		[HttpPut("{id:Guid}")]
		public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] UpdateQuestionRequest request)
		{
			var result = await _questionService.UpdateQuestionAsync(id, request);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		[HttpDelete("{id:Guid}")]
		public async Task<IActionResult> DeleteQuestion(Guid id)
		{
			var result = await _questionService.DeleteQuestionAsync(id);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		[HttpGet("export-pdf/{lessonPlanId:Guid}")]
		public async Task<IActionResult> ExportQuestionsPdf(Guid lessonPlanId, [FromQuery] GetQuestionsRequest? filterRequest = null)
		{
			try
			{
				var pdfBytes = await _questionService.ExportQuestionsPdfAsync(lessonPlanId, filterRequest);
				return File(pdfBytes, "application/pdf", $"questions-{lessonPlanId}.pdf");
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = $"Không thể xuất PDF: {ex.Message}" });
			}
		}
	}
}
