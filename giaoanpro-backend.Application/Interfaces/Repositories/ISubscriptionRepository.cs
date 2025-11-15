using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface ISubscriptionRepository : IGenericRepository<Subscription>
	{
		Task<Subscription?> GetByIdAndUserAsync(Guid subscriptionId, Guid userId, bool includePlan = false, bool includePayments = false);
		Task<Subscription?> GetPendingRetryAsync(Guid subscriptionId, Guid userId);
		Task<Subscription?> GetCurrentAccessByUserAsync(Guid userId);
		Task<IEnumerable<Subscription>> GetHistoryByUserIdAsync(Guid userId);
		Task<bool> UserHasActiveSubscriptionAsync(Guid userId);
	}
}
