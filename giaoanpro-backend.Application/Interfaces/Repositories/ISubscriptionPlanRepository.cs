using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface ISubscriptionPlanRepository : IGenericRepository<SubscriptionPlan>
	{
		Task<IEnumerable<SubscriptionPlan>> GetSubscriptionPlansAsync(bool onlyActive = false);
		Task<SubscriptionPlan?> GetPlanByIdAsync(Guid id);
		Task<(IEnumerable<SubscriptionPlan> Items, int TotalCount)> GetSubscriptionPlansAsync(
			string? search,
			decimal? minPrice,
			decimal? maxPrice,
			int? minDurationInDays,
			int? maxDurationInDays,
			int? minLessons,
			int? minPromptsPerDay,
			bool? isActive,
			string? sortBy,
			bool isDescending,
			int pageNumber,
			int pageSize);

		Task<bool> HasSubscriptionsAsync(Guid planId);
	}
}
