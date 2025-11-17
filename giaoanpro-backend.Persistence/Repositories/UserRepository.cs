using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Domain.Enums;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Extensions;
using giaoanpro_backend.Persistence.Repositories.Bases;
using System.Linq.Expressions;

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

		public async Task<IEnumerable<User>> GetUsersAsync(bool includeInactive = false, bool includeAdmins = false)
		{
			Expression<Func<User, bool>>? filter = null;

			if (!includeInactive)
			{
				Expression<Func<User, bool>> activeFilter = u => u.IsActive;
				filter = activeFilter;
			}

			if (!includeAdmins)
			{
				Expression<Func<User, bool>> notAdminFilter = u => u.Role != UserRole.Admin;
				if (filter is null)
					filter = notAdminFilter;
				else
					filter = filter.AndAlso(notAdminFilter);
			}

			return await GetAllAsync(filter: filter, asNoTracking: true);
		}
	}
}
