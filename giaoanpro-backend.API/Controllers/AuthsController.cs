using giaoanpro_backend.Application.DTOs.Requests.Auths;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthsController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthsController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			var result = await _authService.LoginAsync(request);
			return result.Success ? Ok(result) : Unauthorized(result);
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequest request, [FromQuery] UserRole role)
		{
			if (role == UserRole.Admin)
			{
				return Forbid("Cannot register as Admin role.");
			}
			var result = await _authService.RegisterAsync(request, role);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		[HttpPost("admin/register")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> RegisterByAdmin([FromBody] RegisterRequest request, [FromQuery] UserRole role)
		{
			var result = await _authService.RegisterAsync(request, role);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		[HttpPost("google-login")]
		public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request, [FromQuery] UserRole? role = null)
		{
			var result = await _authService.LoginWithGoogleAsync(request, role);
			if (!result.Success)
				return Unauthorized(result);

			if (result.Payload is not null && result.Payload.Role == UserRole.Admin)
			{
				return Forbid("Admin role cannot be provisioned via Google login.");
			}

			return Ok(result);
		}

		[HttpGet("Test")]
		[Authorize]
		public IActionResult Test()
		{
			return Ok("API is working!");
		}
	}
}