using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface ISyllabusRepository : IGenericRepository<Syllabus>
	{
		Task<Syllabus?> GetBySubjectIdAsync(Guid subjectId);
		Task<bool> ExistsBySubjectIdAsync(Guid subjectId, Guid? excludeId = null);
	}
}
