using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Repositories.Bases;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Persistence.Repositories
{
	public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
	{
		public PaymentRepository(GiaoanproDBContext context) : base(context)
		{
		}

		public async Task<IEnumerable<Payment>> GetBySubscriptionIdAsync(Guid subscriptionId)
		{
			return await GetAllAsync(p => p.SubscriptionId == subscriptionId);
		}

		public async Task<IEnumerable<Payment>> GetHistoryByUserIdAsync(Guid userId)
		{
			return await GetAllAsync(
				filter: p => p.Subscription != null && p.Subscription.UserId == userId,
				include: q => q.Include(p => p.Subscription).ThenInclude(s => s.Plan),
				orderBy: q => q.OrderByDescending(p => p.PaymentDate),
				asNoTracking: true
			);
		}

		public async Task<Payment?> GetByIdWithSubscriptionDetailsAsync(Guid paymentId)
		{
			return await FirstOrDefaultAsync(
				predicate: p => p.Id == paymentId,
				include: q => q
					.Include(p => p.Subscription)
						.ThenInclude(s => s.User)
					.Include(p => p.Subscription)
						.ThenInclude(s => s.Plan),
				asNoTracking: true
			);
		}
	}
}
