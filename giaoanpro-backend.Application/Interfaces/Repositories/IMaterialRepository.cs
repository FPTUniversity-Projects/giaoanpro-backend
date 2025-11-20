using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface IMaterialRepository : IGenericRepository<Material>
	{
		Task<IEnumerable<Material>> GetByActivityIdAsync(Guid activityId);
	}
}
