using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Domain.Enums;
using Google.Apis.Auth;
using System.Security.Claims;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface IAuthRepository
	{
		public string GenerateJwtToken(User user, string role);
		public ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);
		public Task<User> RegisterViaGoogleAsync(GoogleJsonWebSignature.Payload payload, UserRole? preferredRole = null);
		// Generate a refresh token, persist it to the user and return the token string
		public Task<string> GenerateAndSaveRefreshToken(User user);
		// Validate refresh token for a user and return the user if valid, otherwise null
		public Task<User?> ValidateRefreshToken(Guid userId, string refreshToken);
		// Revoke (clear) refresh token for a user
		public Task<bool> RevokeRefreshToken(Guid userId);
	}
}
