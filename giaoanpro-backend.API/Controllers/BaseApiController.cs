using giaoanpro_backend.Application.DTOs.Responses.Bases;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace giaoanpro_backend.API.Controllers
{
	[ApiController]
	public abstract class BaseApiController : ControllerBase
	{
		/// <summary>
		/// Lấy UserId của user hiện tại từ token
		/// </summary> 
		protected Guid GetCurrentUserId()
		{
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
		}

		/// <summary>
		/// Xử lý response cho các hàm có payload (generic)
		/// </summary>
		protected ActionResult HandleResponse<T>(BaseResponse<T> result)
		{
			if (result.Success)
			{
				return Ok(result);
			}

			return result.ErrorType switch
			{
				ResponseErrorType.NotFound => NotFound(result),
				ResponseErrorType.BadRequest => BadRequest(result),
				ResponseErrorType.Conflict => Conflict(result),
				ResponseErrorType.Unauthorized => Unauthorized(result),
				ResponseErrorType.Forbidden => StatusCode(403, result),
				ResponseErrorType.InternalError => StatusCode(500, result),
				_ => BadRequest(result)
			};
		}

		/// <summary>
		/// Xử lý response cho các hàm không có payload (non-generic)
		/// </summary>
		protected ActionResult HandleResponse(BaseResponse result)
		{
			if (result.Success)
			{
				return Ok(result);
			}

			return result.ErrorType switch
			{
				ResponseErrorType.NotFound => NotFound(result),
				ResponseErrorType.BadRequest => BadRequest(result),
				ResponseErrorType.Conflict => Conflict(result),
				ResponseErrorType.Unauthorized => Unauthorized(result),
				ResponseErrorType.Forbidden => StatusCode(403, result),
				ResponseErrorType.InternalError => StatusCode(500, result),
				_ => BadRequest(result)
			};
		}
	}
}
