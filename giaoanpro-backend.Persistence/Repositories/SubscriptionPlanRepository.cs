using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Extensions;
using giaoanpro_backend.Persistence.Repositories.Bases;
using System.Linq.Expressions;

namespace giaoanpro_backend.Persistence.Repositories
{
	public class SubscriptionPlanRepository : GenericRepository<SubscriptionPlan>, ISubscriptionPlanRepository
	{
		public SubscriptionPlanRepository(GiaoanproDBContext context) : base(context)
		{
		}

		public async Task<IEnumerable<SubscriptionPlan>> GetSubscriptionPlansAsync(bool onlyActive = false)
		{
			if (onlyActive)
				return await GetAllAsync(filter: p => p.IsActive);
			return await GetAllAsync();
		}

		public async Task<SubscriptionPlan?> GetPlanByIdAsync(Guid id)
		{
			return await GetByIdAsync(id);
		}

		public async Task<(IEnumerable<SubscriptionPlan> Items, int TotalCount)> GetSubscriptionPlansAsync(
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
			int pageSize)
		{
			Expression<Func<SubscriptionPlan, bool>>? filter = null;

			if (!string.IsNullOrWhiteSpace(search))
			{
				var term = search.Trim();
				Expression<Func<SubscriptionPlan, bool>> searchFilter = p => p.Name.Contains(term) || p.Description.Contains(term);
				filter = filter is null ? searchFilter : filter.AndAlso(searchFilter);
			}

			if (minPrice.HasValue)
			{
				Expression<Func<SubscriptionPlan, bool>> minPriceFilter = p => p.Price >= minPrice.Value;
				filter = filter is null ? minPriceFilter : filter.AndAlso(minPriceFilter);
			}

			if (maxPrice.HasValue)
			{
				Expression<Func<SubscriptionPlan, bool>> maxPriceFilter = p => p.Price <= maxPrice.Value;
				filter = filter is null ? maxPriceFilter : filter.AndAlso(maxPriceFilter);
			}

			if (minDurationInDays.HasValue)
			{
				Expression<Func<SubscriptionPlan, bool>> minDurFilter = p => p.DurationInDays >= minDurationInDays.Value;
				filter = filter is null ? minDurFilter : filter.AndAlso(minDurFilter);
			}

			if (maxDurationInDays.HasValue)
			{
				Expression<Func<SubscriptionPlan, bool>> maxDurFilter = p => p.DurationInDays <= maxDurationInDays.Value;
				filter = filter is null ? maxDurFilter : filter.AndAlso(maxDurFilter);
			}

			if (minLessons.HasValue)
			{
				Expression<Func<SubscriptionPlan, bool>> minLessonsFilter = p => p.MaxLessonPlans >= minLessons.Value;
				filter = filter is null ? minLessonsFilter : filter.AndAlso(minLessonsFilter);
			}

			if (minPromptsPerDay.HasValue)
			{
				Expression<Func<SubscriptionPlan, bool>> minPromptsFilter = p => p.MaxPromptsPerDay >= minPromptsPerDay.Value;
				filter = filter is null ? minPromptsFilter : filter.AndAlso(minPromptsFilter);
			}

			if (isActive.HasValue)
			{
				Expression<Func<SubscriptionPlan, bool>> isActiveFilter = p => p.IsActive == isActive.Value;
				filter = filter is null ? isActiveFilter : filter.AndAlso(isActiveFilter);
			}

			var result = await GetPagedAsync(
				filter: filter,
				include: null,
				orderBy: q => q.ApplySorting(sortBy, isDescending),
				pageNumber: pageNumber,
				pageSize: pageSize,
				asNoTracking: true
			);

			return result;
		}

		public async Task<bool> HasSubscriptionsAsync(Guid planId)
		{
			return await AnyAsync(p => p.Id == planId && p.UserSubscriptions.Any());
		}
	}
}
