using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Repositories.Bases;

namespace giaoanpro_backend.Persistence.Repositories
{
	public class SubscriptionPlanRepository : GenericRepository<SubscriptionPlan>, ISubscriptionPlanRepository
	{
		public SubscriptionPlanRepository(GiaoanproDBContext context) : base(context)
		{
		}

		public async Task<IEnumerable<SubscriptionPlan>> GetAllPlansAsync(bool onlyActive = false)
		{
			if (onlyActive)
				return await GetAllAsync(filter: p => p.IsActive);
			return await GetAllAsync();
		}

		public async Task<SubscriptionPlan?> GetPlanByIdAsync(Guid id)
		{
			return await GetByIdAsync(id);
		}
	}
}
