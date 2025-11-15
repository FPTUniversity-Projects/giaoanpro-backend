using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface IPaymentRepository : IGenericRepository<Payment>
	{
		Task<IEnumerable<Payment>> GetHistoryByUserIdAsync(Guid userId);
		Task<Payment?> GetByIdWithSubscriptionDetailsAsync(Guid paymentId);
	}
}
