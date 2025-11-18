using giaoanpro_backend.Application.DTOs.Requests.Auths;
using giaoanpro_backend.Application.DTOs.Responses.Auths;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Application.Interfaces.Services
{
	public interface IAuthService
	{
		Task<BaseResponse<string>> RegisterAsync(RegisterRequest register, UserRole role);
		Task<BaseResponse<TokenResponse>> LoginAsync(LoginRequest login);
		Task<BaseResponse<TokenResponse>> LoginWithGoogleAsync(GoogleLoginRequest request, UserRole? preferredRole = null);
		Task<BaseResponse<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request);
		Task<BaseResponse<string>> RevokeRefreshTokenAsync(Guid userId);
	}
}
