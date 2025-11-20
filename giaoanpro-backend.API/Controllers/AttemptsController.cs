using System;
using System.Threading.Tasks;
using giaoanpro_backend.Application.DTOs.Requests.Attempts;
using giaoanpro_backend.Application.DTOs.Responses.Attempts;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
    [Route("api/attempts")]
    [ApiController]
    [Authorize]
    public class AttemptsController : BaseApiController
    {
        private readonly IAttemptService _attemptService;

        public AttemptsController(IAttemptService attemptService)
        {
            _attemptService = attemptService;
        }

        [HttpPost("start")]
        [ProducesResponseType(typeof(BaseResponse<ExamPaperResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<ExamPaperResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<ExamPaperResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<ExamPaperResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BaseResponse<ExamPaperResponse>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BaseResponse<ExamPaperResponse>>> StartAttempt([FromBody] StartAttemptRequest request)
        {
            var validation = ValidateRequestBody<ExamPaperResponse>(request);
            if (validation != null) return validation;

            var userId = GetCurrentUserId();
            var result = await _attemptService.StartAttemptAsync(request, userId);
            return HandleResponse(result);
        }

        [HttpPut("progress")]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BaseResponse<string>>> SaveProgress([FromBody] UpdateProgressRequest request)
        {
            var validation = ValidateRequestBody<string>(request);
            if (validation != null) return validation;

            var userId = GetCurrentUserId();
            var result = await _attemptService.SaveProgressAsync(request, userId);
            return HandleResponse(result);
        }

        [HttpPost("submit")]
        [ProducesResponseType(typeof(BaseResponse<AttemptResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<AttemptResultResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<AttemptResultResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<AttemptResultResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BaseResponse<AttemptResultResponse>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BaseResponse<AttemptResultResponse>>> SubmitAttempt([FromBody] SubmitAttemptRequest request)
        {
            var validation = ValidateRequestBody<AttemptResultResponse>(request);
            if (validation != null) return validation;

            var userId = GetCurrentUserId();
            var result = await _attemptService.SubmitAttemptAsync(request, userId);
            return HandleResponse(result);
        }

        [HttpPost("grade")]
        [Authorize(Roles = "Teacher, Admin")]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BaseResponse<string>>> GradeAttempt([FromBody] GradeAttemptRequest request)
        {
            var validation = ValidateRequestBody<string>(request);
            if (validation != null) return validation;

            var userId = GetCurrentUserId();
            var result = await _attemptService.GradeAttemptAsync(request, userId);
            return HandleResponse(result);
        }

        // Student history
        [HttpGet("my-history")]
        [ProducesResponseType(typeof(BaseResponse<List<AttemptSummaryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<List<AttemptSummaryResponse>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<List<AttemptSummaryResponse>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<List<AttemptSummaryResponse>>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BaseResponse<List<AttemptSummaryResponse>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BaseResponse<List<AttemptSummaryResponse>>>> GetMyHistory()
        {
            var userId = GetCurrentUserId();
            var result = await _attemptService.GetMyAttemptsAsync(userId);
            return HandleResponse(result);
        }

        // Teacher view: attempts for an exam
        [HttpGet("exam/{examId:guid}")]
        [Authorize(Roles = "Teacher, Admin")]
        [ProducesResponseType(typeof(BaseResponse<List<AttemptSummaryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<List<AttemptSummaryResponse>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<List<AttemptSummaryResponse>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<List<AttemptSummaryResponse>>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BaseResponse<List<AttemptSummaryResponse>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BaseResponse<List<AttemptSummaryResponse>>>> GetAttemptsByExam([FromRoute] Guid examId)
        {
            var userId = GetCurrentUserId();
            var result = await _attemptService.GetAttemptsByExamAsync(examId, userId);
            return HandleResponse(result);
        }

        // Attempt detail / review
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BaseResponse<AttemptReviewResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<AttemptReviewResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<AttemptReviewResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<AttemptReviewResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BaseResponse<AttemptReviewResponse>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BaseResponse<AttemptReviewResponse>>> GetAttemptDetail([FromRoute] Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _attemptService.GetAttemptDetailAsync(id, userId);
            return HandleResponse(result);
        }
    }
}
