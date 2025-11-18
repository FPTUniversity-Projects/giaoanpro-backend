using giaoanpro_backend.Application.DTOs.Requests.Auths;
using giaoanpro_backend.Application.DTOs.Responses.Auths;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthsController : BaseApiController
	{
		private readonly IAuthService _authService;

		public AuthsController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("login")]
		[ProducesResponseType(typeof(BaseResponse<TokenResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<TokenResponse>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(BaseResponse<TokenResponse>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(BaseResponse<TokenResponse>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(BaseResponse<TokenResponse>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<TokenResponse>>> Login([FromBody] LoginRequest request)
		{
			var validation = ValidateRequestBody<TokenResponse>(request);
			if (validation != null)
			{
				return validation;
			}

			var result = await _authService.LoginAsync(request);
			return HandleResponse(result);
		}

		[HttpPost("register")]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<string>>> Register([FromBody] RegisterRequest request, [FromQuery] UserRole role)
		{
			var validation = ValidateRequestBody<string>(request);
			if (validation != null)
			{
				return validation;
			}

			if (role == UserRole.Admin)
			{
				return Forbid("Cannot register as Admin role.");
			}
			var result = await _authService.RegisterAsync(request, role);
			return HandleResponse(result);
		}

		[HttpPost("admin/register")]
		[Authorize(Roles = "Admin")]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<string>>> RegisterByAdmin([FromBody] RegisterRequest request, [FromQuery] UserRole role)
		{
			var validation = ValidateRequestBody<string>(request);
			if (validation != null)
			{
				return validation;
			}

			var result = await _authService.RegisterAsync(request, role);
			return HandleResponse(result);
		}

		[HttpPost("google-login")]
		[ProducesResponseType(typeof(BaseResponse<TokenResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<TokenResponse>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(BaseResponse<TokenResponse>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(BaseResponse<TokenResponse>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<TokenResponse>>> GoogleLogin([FromBody] GoogleLoginRequest request, [FromQuery] UserRole? role = null)
		{
			var validation = ValidateRequestBody<TokenResponse>(request);
			if (validation != null)
			{
				return validation;
			}

			var result = await _authService.LoginWithGoogleAsync(request, role);
			if (!result.Success)
				return HandleResponse(result);

			if (result.Payload is not null && result.Payload.Role == UserRole.Admin)
			{
				return Forbid("Admin role cannot be provisioned via Google login.");
			}

			return HandleResponse(result);
		}
	}
}