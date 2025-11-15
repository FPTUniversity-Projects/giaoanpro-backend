using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface ISubscriptionPlanRepository : IGenericRepository<SubscriptionPlan>
	{
		Task<IEnumerable<SubscriptionPlan>> GetAllPlansAsync(bool onlyActive = false);
		Task<SubscriptionPlan?> GetPlanByIdAsync(Guid id);
	}
}
