using giaoanpro_backend.Application.DTOs.Requests.Users;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Users;
using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Application.Interfaces.Services;
using MapsterMapper;

namespace giaoanpro_backend.Application.Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepository;
		private readonly IMapper _mapper;

		public UserService(IUserRepository userRepository, IMapper mapper)
		{
			_userRepository = userRepository;
			_mapper = mapper;
		}

		public async Task<BaseResponse<List<GetUserLookupResponse>>> GetUserLookupsAsync(GetUserLookupRequest request)
		{
			var users = await _userRepository.GetUsersAsync(request.IncludeInactive, request.IncludeAdmins);
			return BaseResponse<List<GetUserLookupResponse>>.Ok(
				_mapper.Map<List<GetUserLookupResponse>>(users),
				"User lookups retrieved successfully.");
		}
	}
}
