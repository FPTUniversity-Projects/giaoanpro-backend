using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Application.DTOs.Requests.ExamMatrices;
using giaoanpro_backend.Application.DTOs.Responses.ExamMatrices;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace giaoanpro_backend.API.Controllers
{
    [Route("api/exam-matrices")]
    [ApiController]
    [Authorize(Roles = "Admin, Teacher")]
    public class ExamMatricesController : BaseApiController
    {
        private readonly IExamMatrixService _examMatrixService;

        public ExamMatricesController(IExamMatrixService examMatrixService)
        {
            _examMatrixService = examMatrixService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<GetMatrixResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<GetMatrixResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<GetMatrixResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<GetMatrixResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BaseResponse<GetMatrixResponse>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BaseResponse<GetMatrixResponse>>> CreateMatrix([FromBody] CreateMatrixRequest request)
        {
            var validation = ValidateRequestBody<GetMatrixResponse>(request);
            if (validation != null) return validation;

            var result = await _examMatrixService.CreateMatrixAsync(request);
            return HandleResponse(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BaseResponse<GetMatrixDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<GetMatrixDetailResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<GetMatrixDetailResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<GetMatrixDetailResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BaseResponse<GetMatrixDetailResponse>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BaseResponse<GetMatrixDetailResponse>>> GetMatrixById([FromRoute] System.Guid id)
        {
            var result = await _examMatrixService.GetMatrixByIdAsync(id);
            return HandleResponse(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<GetMatrixPagedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<GetMatrixPagedResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<GetMatrixPagedResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<GetMatrixPagedResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BaseResponse<GetMatrixPagedResponse>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BaseResponse<GetMatrixPagedResponse>>> GetMatrices([FromQuery] GetMatrixPagedRequest request)
        {
            var result = await _examMatrixService.GetMatricesPagedAsync(request);
            return HandleResponse(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BaseResponse<string>>> UpdateMatrix([FromRoute] System.Guid id, [FromBody] UpdateMatrixRequest request)
        {
            var validation = ValidateRequestBody<string>(request);
            if (validation != null) return validation;

            if (id != request.Id)
            {
                var mismatch = BaseResponse<string>.Fail("Route id does not match request id.", ResponseErrorType.BadRequest);
                return HandleResponse(mismatch);
            }

            var result = await _examMatrixService.UpdateMatrixAsync(request);
            return HandleResponse(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BaseResponse<string>>> DeleteMatrix([FromRoute] System.Guid id)
        {
            var result = await _examMatrixService.DeleteMatrixAsync(id);
            return HandleResponse(result);
        }
    }
}
