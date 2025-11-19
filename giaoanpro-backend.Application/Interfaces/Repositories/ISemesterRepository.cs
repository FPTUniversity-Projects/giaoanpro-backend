using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface ISemesterRepository : IGenericRepository<Semester>
	{
		Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
		Task<bool> HasDateOverlapAsync(DateTime startDate, DateTime endDate, Guid? excludeId = null);
		Task<IEnumerable<Semester>> GetActiveSemestersAsync(DateTime currentDate);
	}
}
