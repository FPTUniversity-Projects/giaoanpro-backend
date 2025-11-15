using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface IUserRepository : IGenericRepository<User>
	{
		Task<User?> GetByEmailAsync(string email);
		Task<User?> GetByEmailOrUsernameAsync(string email, string username);
	}
}
