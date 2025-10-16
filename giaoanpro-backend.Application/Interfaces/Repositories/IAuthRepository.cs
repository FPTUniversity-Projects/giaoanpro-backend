using giaoanpro_backend.Domain.Entities;
using Google.Apis.Auth;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface IAuthRepository
	{
		public Task<string> GenerateJwtToken(User user, string role);
		public Task<User> RegisterViaGoogleAsync(GoogleJsonWebSignature.Payload payload);
	}
}
