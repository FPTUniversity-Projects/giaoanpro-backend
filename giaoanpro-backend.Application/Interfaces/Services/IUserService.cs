using giaoanpro_backend.Application.DTOs.Requests.Users;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Users;

namespace giaoanpro_backend.Application.Interfaces.Services
{
	public interface IUserService
	{
		Task<BaseResponse<PagedResult<GetUserResponse>>> GetUsersAsync(GetUsersQuery query);
		Task<BaseResponse<GetUserResponse>> GetUserByIdAsync(Guid id);
		Task<BaseResponse<GetUserResponse>> GetCurrentUserAsync(Guid userId);
		Task<BaseResponse<GetUserResponse>> UpdateUserStatusAsync(Guid id, bool isActive);
		Task<BaseResponse<GetUserResponse>> UpdateCurrentUserAsync(Guid userId, UpdateCurrentUserRequest request);
		Task<BaseResponse<List<GetUserLookupResponse>>> GetUserLookupsAsync(GetUserLookupRequest request);
	}
}
