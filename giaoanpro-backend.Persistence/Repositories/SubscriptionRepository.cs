using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Repositories.Bases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Persistence.Repositories
{
	public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
	{
		public SubscriptionRepository(GiaoanproDBContext context) : base(context)
		{
		}

		public async Task<Subscription?> GetByIdAndUserAsync(Guid subscriptionId, Guid userId, bool includePlan = false, bool includePayments = false)
		{
			Func<IQueryable<Subscription>, IIncludableQueryable<Subscription, object>>? include = null;

			if (includePlan && includePayments)
			{
				include = q => q.Include(s => s.Plan).Include(s => s.Payments);
			}
			else if (includePlan)
			{
				include = q => q.Include(s => s.Plan);
			}
			else if (includePayments)
			{
				include = q => q.Include(s => s.Payments);
			}

			return await GetByConditionAsync(s => s.Id == subscriptionId && s.UserId == userId, include);
		}

		public async Task<Subscription?> GetPendingRetryAsync(Guid subscriptionId, Guid userId)
		{
			return await GetByConditionAsync(s => s.Id == subscriptionId && s.UserId == userId && s.Status == SubscriptionStatus.Inactive);
		}

		public async Task<Subscription?> GetCurrentAccessByUserAsync(Guid userId)
		{
			return await GetByConditionAsync(s => s.UserId == userId && (s.Status == SubscriptionStatus.Active || (s.Status == SubscriptionStatus.Canceled && s.EndDate >= DateTime.UtcNow)));
		}

		public async Task<IEnumerable<Subscription>> GetHistoryByUserIdAsync(Guid userId)
		{
			return await GetAllAsync(s => s.UserId == userId);
		}

		public async Task<bool> UserHasActiveSubscriptionAsync(Guid userId)
		{
			return await AnyAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);
		}
	}
}
