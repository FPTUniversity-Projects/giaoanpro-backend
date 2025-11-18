using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Repositories.Bases;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Persistence.Repositories
{
	public class GradeRepository : GenericRepository<Grade>, IGradeRepository
	{
		private readonly GiaoanproDBContext _context;

		public GradeRepository(GiaoanproDBContext context) : base(context)
		{
			_context = context;
		}

		public async Task<bool> ExistsByLevelAsync(int level, Guid? excludeId = null)
		{
			var query = _context.Grades.Where(g => g.Level == level);
			
			if (excludeId.HasValue)
			{
				query = query.Where(g => g.Id != excludeId.Value);
			}

			return await query.AnyAsync();
		}

		public async Task<Grade?> GetByLevelAsync(int level)
		{
			return await _context.Grades
				.Include(g => g.Subjects)
				.Include(g => g.Classes)
				.FirstOrDefaultAsync(g => g.Level == level);
		}
	}
}
