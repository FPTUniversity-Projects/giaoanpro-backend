using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Repositories.Bases;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Persistence.Repositories
{
	public class SemesterRepository : GenericRepository<Semester>, ISemesterRepository
	{
		private readonly GiaoanproDBContext _context;

		public SemesterRepository(GiaoanproDBContext context) : base(context)
		{
			_context = context;
		}

		public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
		{
			var query = _context.Semesters.Where(s => s.Name.ToLower() == name.ToLower());
			
			if (excludeId.HasValue)
			{
				query = query.Where(s => s.Id != excludeId.Value);
			}

			return await query.AnyAsync();
		}

		public async Task<bool> HasDateOverlapAsync(DateTime startDate, DateTime endDate, Guid? excludeId = null)
		{
			var query = _context.Semesters.Where(s =>
				(s.StartDate <= endDate && s.EndDate >= startDate)
			);

			if (excludeId.HasValue)
			{
				query = query.Where(s => s.Id != excludeId.Value);
			}

			return await query.AnyAsync();
		}

		public async Task<IEnumerable<Semester>> GetActiveSemestersAsync(DateTime currentDate)
		{
			return await _context.Semesters
				.Where(s => s.StartDate <= currentDate && s.EndDate >= currentDate)
				.OrderBy(s => s.StartDate)
				.ToListAsync();
		}
	}
}
