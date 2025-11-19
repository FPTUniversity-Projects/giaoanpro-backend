using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface IActivityRepository : IGenericRepository<Activity>
	{
		Task<Activity?> GetByIdWithChildrenAsync(Guid id);
		Task<IEnumerable<Activity>> GetByLessonPlanIdAsync(Guid lessonPlanId);
	}
}
