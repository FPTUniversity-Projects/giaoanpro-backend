using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.Users;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Users;
using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Domain.Entities;
using MapsterMapper;
using System.Linq.Expressions;

namespace giaoanpro_backend.Application.Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepository;
		private readonly IAuthService _authService;
		private readonly IMapper _mapper;
		private readonly IValidator<UpdateCurrentUserRequest> _updateCurrentUserValidator;

		public UserService(IUserRepository userRepository, IMapper mapper, IValidator<UpdateCurrentUserRequest> updateCurrentUserValidator, IAuthService authService)
		{
			_userRepository = userRepository;
			_mapper = mapper;
			_updateCurrentUserValidator = updateCurrentUserValidator;
			_authService = authService;
		}

		public async Task<BaseResponse<List<GetUserLookupResponse>>> GetUserLookupsAsync(GetUserLookupRequest request)
		{
			var users = await _userRepository.GetUsersAsync(request.IncludeInactive, request.IncludeTeacherOnly);
			return BaseResponse<List<GetUserLookupResponse>>.Ok(
				_mapper.Map<List<GetUserLookupResponse>>(users),
				"User lookups retrieved successfully.");
		}

		public async Task<BaseResponse<PagedResult<GetUserResponse>>> GetUsersAsync(GetUsersQuery query)
		{
			// Basic paging defaults
			int pageNumber = query?.PageNumber > 0 ? query.PageNumber : 1;
			int pageSize = query?.PageSize > 0 ? Math.Min(query.PageSize, 50) : 10;

			// Build filters
			Expression<Func<User, bool>>? filter = null;
			if (!string.IsNullOrWhiteSpace(query?.Search))
			{
				var term = query.Search.Trim();
				Expression<Func<User, bool>> searchFilter = u => u.Email.Contains(term) || u.FullName.Contains(term) || u.Username.Contains(term);
				filter = searchFilter;
			}

			bool includeInactive = query?.IncludeInactive ?? false;
			if (!includeInactive)
			{
				Expression<Func<User, bool>> activeFilter = u => u.IsActive;
				filter = filter is null ? activeFilter : AndAlso(filter, activeFilter);
			}

			if (query?.TeacherOnly == true)
			{
				Expression<Func<User, bool>> teacherFilter = u => u.Role == Domain.Enums.UserRole.Teacher;
				filter = filter is null ? teacherFilter : AndAlso(filter, teacherFilter);
			}

			var (items, total) = await _userRepository.GetPagedAsync(filter: filter, include: null, orderBy: null, pageNumber: pageNumber, pageSize: pageSize, asNoTracking: true);

			var mapped = _mapper.Map<List<GetUserResponse>>(items);
			var paged = new PagedResult<GetUserResponse>(mapped, pageNumber, pageSize, total);

			return BaseResponse<PagedResult<GetUserResponse>>.Ok(paged, "Users retrieved successfully.");
		}

		public async Task<BaseResponse<GetUserResponse>> GetUserByIdAsync(Guid id)
		{
			if (id == Guid.Empty) return BaseResponse<GetUserResponse>.Fail("Invalid user id.", ResponseErrorType.BadRequest);
			var user = await _userRepository.GetByIdAsync(id);
			if (user == null) return BaseResponse<GetUserResponse>.Fail("User not found.", ResponseErrorType.NotFound);
			var dto = _mapper.Map<GetUserResponse>(user);
			return BaseResponse<GetUserResponse>.Ok(dto);
		}

		public async Task<BaseResponse<GetUserResponse>> GetCurrentUserAsync(Guid userId)
		{
			return await GetUserByIdAsync(userId);
		}

		public async Task<BaseResponse<GetUserResponse>> UpdateUserStatusAsync(Guid id, bool isActive)
		{
			if (id == Guid.Empty) return BaseResponse<GetUserResponse>.Fail("Invalid user id.", ResponseErrorType.BadRequest);
			var user = await _userRepository.GetByIdAsync(id);
			if (user == null) return BaseResponse<GetUserResponse>.Fail("User not found.", ResponseErrorType.NotFound);
			user.IsActive = isActive;
			if (!isActive)
			{
				await _authService.RevokeRefreshTokenAsync(user.Id);
			}
			_userRepository.Update(user);
			var saved = await _userRepository.SaveChangesAsync();
			if (!saved) return BaseResponse<GetUserResponse>.Fail("Failed to update user status.", ResponseErrorType.InternalError);
			var dto = _mapper.Map<GetUserResponse>(user);
			return BaseResponse<GetUserResponse>.Ok(dto, "User status updated successfully.");
		}

		public async Task<BaseResponse<GetUserResponse>> UpdateCurrentUserAsync(Guid userId, UpdateCurrentUserRequest request)
		{
			var validationResult = await _updateCurrentUserValidator.ValidateAsync(request);
			if (!validationResult.IsValid)
			{
				var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				return BaseResponse<GetUserResponse>.Fail("Validation failed", ResponseErrorType.BadRequest, errors);
			}

			var user = await _userRepository.GetByIdAsync(userId);
			if (user == null) return BaseResponse<GetUserResponse>.Fail("User not found.", ResponseErrorType.NotFound);

			// Map allowed fields from request to user entity. Keep simple: full name and username.
			if (!string.IsNullOrWhiteSpace(request.FullName)) user.FullName = request.FullName;
			if (!string.IsNullOrWhiteSpace(request.Username)) user.Username = request.Username;
			_userRepository.Update(user);
			var saved = await _userRepository.SaveChangesAsync();
			if (!saved) return BaseResponse<GetUserResponse>.Fail("Failed to update user.", ResponseErrorType.InternalError);
			var dto = _mapper.Map<GetUserResponse>(user);
			return BaseResponse<GetUserResponse>.Ok(dto, "User updated successfully.");
		}

		// Local helper to combine two expression predicates with logical AND.
		private static Expression<Func<T, bool>> AndAlso<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
		{
			var parameter = Expression.Parameter(typeof(T));
			var body = Expression.AndAlso(Expression.Invoke(left, parameter), Expression.Invoke(right, parameter));
			return Expression.Lambda<Func<T, bool>>(body, parameter);
		}
	}
}
