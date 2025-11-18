using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface IGradeRepository : IGenericRepository<Grade>
	{
		Task<bool> ExistsByLevelAsync(int level, Guid? excludeId = null);
		Task<Grade?> GetByLevelAsync(int level);
	}
}
