using giaoanpro_backend.Application.DTOs.Requests.Users;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Users;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class UsersController : BaseApiController
	{
		private readonly IUserService _userServcie;

		public UsersController(IUserService userServcie)
		{
			_userServcie = userServcie;
		}

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<BaseResponse<PagedResult<GetUserResponse>>>> GetUsers([FromQuery] GetUsersQuery query)
		{
			var response = await _userServcie.GetUsersAsync(query);
			return HandleResponse(response);
		}

		[HttpGet("{id:guid}")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<BaseResponse<GetUserResponse>>> GetUserById([FromRoute] Guid id)
		{
			var response = await _userServcie.GetUserByIdAsync(id);
			return HandleResponse(response);
		}

		[HttpGet("me")]
		public async Task<ActionResult<BaseResponse<GetUserResponse>>> GetCurrentUser()
		{
			var userId = GetCurrentUserId();
			var response = await _userServcie.GetCurrentUserAsync(userId);
			return HandleResponse(response);
		}

		[HttpGet("lookup")]
		public async Task<ActionResult<BaseResponse<List<GetUserLookupResponse>>>> GetUserLookups([FromQuery] GetUserLookupRequest request)
		{
			var response = await _userServcie.GetUserLookupsAsync(request);
			return HandleResponse(response);
		}

		[HttpPut("{id:guid}/status")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<BaseResponse<GetUserResponse>>> UpdateUserStatus([FromRoute] Guid id, [FromBody] bool isActive)
		{
			var adminId = GetCurrentUserId();
			if (adminId == id)
			{
				return HandleResponse(BaseResponse<GetUserResponse>.Fail("Admin cannot change their own status.", ResponseErrorType.Forbidden));
			}
			var response = await _userServcie.UpdateUserStatusAsync(id, isActive);
			return HandleResponse(response);
		}

		[HttpPut("me")]
		public async Task<ActionResult<BaseResponse<GetUserResponse>>> UpdateCurrentUser([FromBody] UpdateCurrentUserRequest request)
		{
			var userId = GetCurrentUserId();
			var response = await _userServcie.UpdateCurrentUserAsync(userId, request);
			return HandleResponse(response);
		}
	}
}
