using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Domain.Enums;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Extensions;
using giaoanpro_backend.Persistence.Repositories.Bases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace giaoanpro_backend.Persistence.Repositories
{
	public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
	{
		public SubscriptionRepository(GiaoanproDBContext context) : base(context)
		{
		}

		public async Task<Subscription?> GetByIdAndUserAsync(Guid subscriptionId, Guid? userId, bool includePlan = false, bool includePayments = false, bool includeUser = false)
		{
			// If userId is null (admin/public retrieval), include related entities by default unless caller explicitly requests otherwise.
			bool includePlanFlag = includePlan || userId == null;
			bool includePaymentsFlag = includePayments || userId == null;
			bool includeUserFlag = includeUser || userId == null;

			Func<IQueryable<Subscription>, IIncludableQueryable<Subscription, object>>? include = null;

			if (includePlanFlag || includePaymentsFlag || includeUserFlag)
			{
				include = q =>
				{
					var inc = q;
					if (includePlanFlag)
						inc = inc.Include(s => s.Plan);
					if (includePaymentsFlag)
						inc = inc.Include(s => s.Payments);
					if (includeUserFlag)
						inc = inc.Include(s => s.User);
					return (IIncludableQueryable<Subscription, object>)inc;
				};
			}

			if (userId == null)
			{
				return await GetByConditionAsync(s => s.Id == subscriptionId, include);
			}

			return await GetByConditionAsync(s => s.Id == subscriptionId && s.UserId == userId, include);
		}

		public async Task<Subscription?> GetPendingRetryAsync(Guid subscriptionId, Guid userId)
		{
			return await GetByConditionAsync(s => s.Id == subscriptionId && s.UserId == userId && s.Status == SubscriptionStatus.Inactive);
		}

		public async Task<Subscription?> GetCurrentAccessByUserAsync(Guid userId)
		{
			return await GetByConditionAsync(s => s.UserId == userId && (s.Status == SubscriptionStatus.Active || (s.Status == SubscriptionStatus.Canceled && s.EndDate >= DateTime.UtcNow)), include: s => s.Include(s => s.Plan));
		}

		public async Task<IEnumerable<Subscription>> GetHistoryByUserIdAsync(Guid userId)
		{
			return await GetAllAsync(s => s.UserId == userId);
		}

		public async Task<bool> UserHasActiveSubscriptionAsync(Guid userId)
		{
			return await AnyAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);
		}

		public async Task<(IEnumerable<Subscription> Items, int TotalCount)> GetSubscriptionsAsync(
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
			int pageSize)
		{
			// Build filter expression
			Expression<Func<Subscription, bool>>? filter = null;

			if (userId.HasValue)
			{
				Expression<Func<Subscription, bool>> userFilter = s => s.UserId == userId.Value;
				filter = filter is null ? userFilter : filter.AndAlso(userFilter);
			}

			if (planId.HasValue)
			{
				Expression<Func<Subscription, bool>> planFilter = s => s.PlanId == planId.Value;
				filter = filter is null ? planFilter : filter.AndAlso(planFilter);
			}

			if (status.HasValue)
			{
				Expression<Func<Subscription, bool>> statusFilter = s => s.Status == status.Value;
				filter = filter is null ? statusFilter : filter.AndAlso(statusFilter);
			}

			if (expiresBefore.HasValue)
			{
				Expression<Func<Subscription, bool>> expBeforeFilter = s => s.EndDate <= expiresBefore.Value;
				filter = filter is null ? expBeforeFilter : filter.AndAlso(expBeforeFilter);
			}

			if (expiresAfter.HasValue)
			{
				Expression<Func<Subscription, bool>> expAfterFilter = s => s.EndDate >= expiresAfter.Value;
				filter = filter is null ? expAfterFilter : filter.AndAlso(expAfterFilter);
			}

			if (minPromptsUsed.HasValue)
			{
				Expression<Func<Subscription, bool>> minPromptsFilter = s => s.CurrentPromptsUsed >= minPromptsUsed.Value;
				filter = filter is null ? minPromptsFilter : filter.AndAlso(minPromptsFilter);
			}

			if (minLessonsCreated.HasValue)
			{
				Expression<Func<Subscription, bool>> minLessonsFilter = s => s.CurrentLessonPlansCreated >= minLessonsCreated.Value;
				filter = filter is null ? minLessonsFilter : filter.AndAlso(minLessonsFilter);
			}

			if (!string.IsNullOrWhiteSpace(search))
			{
				var term = search.Trim();
				Expression<Func<Subscription, bool>> searchFilter = s => (s.User != null && (s.User.Username.Contains(term) || s.User.Email.Contains(term))) || (s.Plan != null && s.Plan.Name.Contains(term));
				filter = filter is null ? searchFilter : filter.AndAlso(searchFilter);
			}

			var result = await GetPagedAsync(
				filter: filter,
				include: q => q.Include(s => s.User).Include(s => s.Plan),
				orderBy: q => q.ApplySorting(sortBy, isDescending),
				pageNumber: pageNumber,
				pageSize: pageSize,
				asNoTracking: true
			);

			return result;
		}
	}
}
