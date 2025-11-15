using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Repositories.Bases;

namespace giaoanpro_backend.Persistence.Repositories
{
	public class UserRepository : GenericRepository<User>, IUserRepository
	{
		public UserRepository(GiaoanproDBContext context) : base(context)
		{
		}

		public async Task<User?> GetByEmailAsync(string email)
		{
			if (string.IsNullOrWhiteSpace(email)) return null;
			return await GetByConditionAsync(u => u.Email == email);
		}

		public async Task<User?> GetByEmailOrUsernameAsync(string email, string username)
		{
			if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(username)) return null;
			return await GetByConditionAsync(u => u.Email == email || u.Username == username);
		}
	}
}
