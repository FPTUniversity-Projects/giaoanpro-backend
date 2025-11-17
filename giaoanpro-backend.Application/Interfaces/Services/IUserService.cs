using giaoanpro_backend.Application.DTOs.Requests.Users;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Users;

namespace giaoanpro_backend.Application.Interfaces.Services
{
	public interface IUserService
	{
		public Task<BaseResponse<List<GetUserLookupResponse>>> GetUserLookupsAsync(GetUserLookupRequest request);
	}
}
