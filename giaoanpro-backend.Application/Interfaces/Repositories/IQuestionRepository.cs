using giaoanpro_backend.Domain.Entities;
using System.Linq.Expressions;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface IQuestionRepository
	{
		Task<(List<Question> Items, int TotalCount)> GetPagedWithOptionsAsync(
			Expression<Func<Question, bool>>? filter,
			int pageNumber,
			int pageSize,
			bool asNoTracking = true);

		Task<Question?> GetByIdWithOptionsAsync(Guid id, bool asNoTracking = true);
	}
}
