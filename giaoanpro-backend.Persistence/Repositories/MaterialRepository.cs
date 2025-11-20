using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Repositories.Bases;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Persistence.Repositories
{
	public class MaterialRepository : GenericRepository<Material>, IMaterialRepository
	{
		private readonly GiaoanproDBContext _db;

		public MaterialRepository(GiaoanproDBContext db) : base(db)
		{
			_db = db;
		}

		public async Task<IEnumerable<Material>> GetByActivityIdAsync(Guid activityId)
		{
			return await _db.Materials
				.Where(m => m.ActivityId == activityId)
				.OrderBy(m => m.CreatedAt)
				.ToListAsync();
		}
	}
}
