using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface ISubscriptionRepository : IGenericRepository<Subscription>
	{
		Task<Subscription?> GetByIdAndUserAsync(Guid subscriptionId, Guid userId, bool includePlan = false, bool includePayments = false);
		Task<Subscription?> GetPendingRetryAsync(Guid subscriptionId, Guid userId);
		Task<Subscription?> GetCurrentAccessByUserAsync(Guid userId);
		Task<IEnumerable<Subscription>> GetHistoryByUserIdAsync(Guid userId);
		Task<bool> UserHasActiveSubscriptionAsync(Guid userId);

		// New: paged query with filters and search (used by service/controller)
		Task<(IEnumerable<Subscription> Items, int TotalCount)> GetSubscriptionsAsync(
			string? search,
			Guid? userId,
			Guid? planId,
			SubscriptionStatus? status,
			DateTime? expiresBefore,
			DateTime? expiresAfter,
			int? minPromptsUsed,
			int? minLessonsCreated,
			string? sortBy,
			bool isDescending,
			int pageNumber,
			int pageSize
		);
	}
}
