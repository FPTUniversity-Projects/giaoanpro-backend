using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Domain.Enums;
using Google.Apis.Auth;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface IAuthRepository
	{
		public string GenerateJwtToken(User user, string role);
		public Task<User> RegisterViaGoogleAsync(GoogleJsonWebSignature.Payload payload, UserRole? preferredRole = null);
	}
}
