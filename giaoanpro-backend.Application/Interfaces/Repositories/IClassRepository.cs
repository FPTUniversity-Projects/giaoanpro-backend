using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface IClassRepository : IGenericRepository<Class>
	{
		Task<bool> ExistsByNameAndSemesterAsync(string name, Guid semesterId, Guid? excludeId = null);
		Task<IEnumerable<Class>> GetByTeacherIdAsync(Guid teacherId);
		Task<IEnumerable<Class>> GetBySemesterIdAsync(Guid semesterId);
		Task<IEnumerable<Class>> GetByGradeIdAsync(Guid gradeId);
		Task<Class?> GetWithMembersAsync(Guid classId);
	}
}
