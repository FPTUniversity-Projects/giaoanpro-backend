using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface ISubjectRepository : IGenericRepository<Subject>
	{
		Task<bool> ExistsByNameAndGradeAsync(string name, Guid gradeId, Guid? excludeId = null);
		Task<IEnumerable<Subject>> GetByGradeIdAsync(Guid gradeId);
	}
}
