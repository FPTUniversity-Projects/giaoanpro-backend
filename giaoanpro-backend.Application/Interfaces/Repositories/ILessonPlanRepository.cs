using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface ILessonPlanRepository : IGenericRepository<LessonPlan>
	{
		Task<LessonPlan?> GetByIdWithActivitiesAsync(Guid id);
		Task<bool> ExistsByTitleAndUserAsync(string title, Guid userId, Guid? excludeId = null);
	}
}
