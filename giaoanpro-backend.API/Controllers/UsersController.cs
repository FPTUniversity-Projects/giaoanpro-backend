using giaoanpro_backend.Application.DTOs.Requests.Users;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Users;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : BaseApiController
	{
		private readonly IUserService _userServcie;

		public UsersController(IUserService userServcie)
		{
			_userServcie = userServcie;
		}

		[HttpGet("lookup")]
		public async Task<ActionResult<BaseResponse<List<GetUserLookupResponse>>>> GetUserLookups([FromQuery] GetUserLookupRequest request)
		{
			var response = await _userServcie.GetUserLookupsAsync(request);
			return HandleResponse(response);
		}
	}
}
